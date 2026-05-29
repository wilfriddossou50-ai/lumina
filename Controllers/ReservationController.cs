using GestionBibliotheque.Constants;
using GestionBibliotheque.Data;
using GestionBibliotheque.Models;
using GestionBibliotheque.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Controllers;

public class ReservationController : Controller
{
    private readonly BibliothequeDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILayoutService _layoutService;

    public ReservationController(
        BibliothequeDbContext context,
        ICurrentUserService currentUser,
        ILayoutService layoutService)
    {
        _context = context;
        _currentUser = currentUser;
        _layoutService = layoutService;
    }

    private async Task<Abonnement?> GetAbonnementActifAsync(int membreId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await _context.Abonnements
            .Include(a => a.IdTypeNavigation)
            .Where(a => a.IdMembre == membreId
                     && a.Statut == AbonnementStatuses.Actif
                     && a.StatutPaiement == PaiementStatuses.Paye
                     && (a.DateFin == null || a.DateFin.Value >= today))
            .OrderByDescending(a => a.IdAbonnment)
            .FirstOrDefaultAsync();
    }

    [HttpGet]
    public async Task<IActionResult> MesReservations()
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Login", "Auth");
        }

        await _layoutService.PopulateAsync(this, "Reservations");

        int membreId = _currentUser.MembreId.Value;
        var reservations = await _context.Reservations
            .Include(r => r.IdLivreNavigation)
            .Where(r => r.IdMembre == membreId)
            .OrderByDescending(r => r.DateReservation)
            .ToListAsync();

        return View(reservations);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Annuler(int id)
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Login", "Auth");
        }

        int membreId = _currentUser.MembreId.Value;
        var reservation = await _context.Reservations
            .Include(r => r.IdLivreNavigation)
            .FirstOrDefaultAsync(r => r.IdReservation == id);

        if (reservation == null || reservation.IdMembre != membreId)
        {
            TempData["Error"] = "Reservation introuvable ou acces refuse.";
            return RedirectToAction(nameof(MesReservations));
        }

        if (reservation.Statut == ReservationStatuses.Honoree)
        {
            TempData["Warning"] = "Cette reservation a deja ete honoree, elle ne peut pas etre annulee.";
            return RedirectToAction(nameof(MesReservations));
        }

        string titreLivre = reservation.IdLivreNavigation?.Titre ?? "ce livre";
        _context.Reservations.Remove(reservation);

        _context.Notifications.Add(new Notification
        {
            IdMembre = membreId,
            IdUtilisateur = null,
            Titre = "Reservation annulee",
            Message = $"Votre reservation pour \"{titreLivre}\" a ete annulee.",
            Type = "reservation_annulee",
            EstLue = false,
            DateCreation = DateTime.Now,
            Lien = "/Reservation/MesReservations"
        });

        await _context.SaveChangesAsync();

        TempData["Success"] = "Reservation annulee.";
        return RedirectToAction(nameof(MesReservations));
    }

    [HttpGet]
    public async Task<IActionResult> Index(string statut = ReservationStatuses.EnAttente)
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        await _layoutService.PopulateAsync(this, "Reservations");

        var query = _context.Reservations
            .Include(r => r.IdLivreNavigation)
            .Include(r => r.IdMembreNavigation)
            .AsQueryable();

        if (statut == ReservationStatuses.EnAttente)
        {
            query = query.Where(r => r.Statut == ReservationStatuses.EnAttente);
        }
        else if (statut == ReservationStatuses.Disponible)
        {
            query = query.Where(r => r.Statut == ReservationStatuses.Disponible);
        }
        else if (statut == ReservationStatuses.Honoree)
        {
            query = query.Where(r => r.Statut == ReservationStatuses.Honoree);
        }

        var reservations = await query
            .OrderBy(r => r.DateReservation)
            .ToListAsync();

        ViewBag.StatutFiltre = statut;
        ViewBag.CountEnAttente = await _context.Reservations.CountAsync(r => r.Statut == ReservationStatuses.EnAttente);
        ViewBag.CountDisponible = await _context.Reservations.CountAsync(r => r.Statut == ReservationStatuses.Disponible);
        ViewBag.CountHonoree = await _context.Reservations.CountAsync(r => r.Statut == ReservationStatuses.Honoree);

        return View(reservations);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConvertirEnEmprunt(int id)
    {
        if (!_currentUser.IsAdmin || !_currentUser.UtilisateurId.HasValue)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        var reservation = await _context.Reservations
            .Include(r => r.IdLivreNavigation)
            .Include(r => r.IdMembreNavigation)
            .FirstOrDefaultAsync(r => r.IdReservation == id);

        if (reservation == null)
        {
            TempData["Error"] = "Reservation introuvable.";
            return RedirectToAction(nameof(Index));
        }

        if (reservation.Statut == ReservationStatuses.Honoree)
        {
            TempData["Warning"] = "Cette reservation a deja ete convertie en emprunt.";
            return RedirectToAction(nameof(Index));
        }

        if (reservation.Statut != ReservationStatuses.Disponible)
        {
            TempData["Warning"] = "Seules les reservations marquees comme disponibles peuvent etre converties.";
            return RedirectToAction(nameof(Index));
        }

        var livre = await _context.Livres.FindAsync(reservation.IdLivre);
        if (livre == null || (livre.QuantiteDisponible ?? 0) <= 0)
        {
            TempData["Error"] = "Le livre n'est pas disponible en stock. Attendez un retour.";
            return RedirectToAction(nameof(Index));
        }

        var abonnement = await GetAbonnementActifAsync(reservation.IdMembre);
        var limites = PlanLimitesConfiguration.GetForTarif(abonnement?.IdTypeNavigation?.Tarif);

        int nbEmpruntsEnCours = await _context.Emprunts
            .CountAsync(e => e.IdMembre == reservation.IdMembre && e.Statut == EmpruntStatuses.EnCours);

        if (nbEmpruntsEnCours >= limites.MaxEmpruntsSimultanes)
        {
            TempData["Error"] = $"Le membre a atteint sa limite de {limites.MaxEmpruntsSimultanes} emprunt(s) simultane(s).";
            return RedirectToAction(nameof(Index));
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var dateRetourPrevu = today.AddDays(limites.DureeEmpruntJours);

        var emprunt = new Emprunt
        {
            IdMembre = reservation.IdMembre,
            DateEmprunt = today,
            IdUserEmprunt = _currentUser.UtilisateurId.Value,
            Statut = EmpruntStatuses.EnCours
        };
        _context.Emprunts.Add(emprunt);
        await _context.SaveChangesAsync();

        var detail = new DetailEmprunt
        {
            IdEmprunt = emprunt.IdEmprunt,
            IdLivre = reservation.IdLivre!.Value,
            DateRetourPrevu = dateRetourPrevu,
            DateRetourReel = null,
            NbJourRetard = 0,
            MontantPenalite = 0
        };
        _context.DetailEmprunts.Add(detail);

        livre.QuantiteDisponible = (livre.QuantiteDisponible ?? 0) - 1;
        if (livre.QuantiteDisponible <= 0)
        {
            livre.Statut = LivreStatuses.Indisponible;
        }

        reservation.Statut = ReservationStatuses.Honoree;

        _context.Notifications.Add(new Notification
        {
            IdMembre = reservation.IdMembre,
            IdUtilisateur = null,
            Titre = "Reservation convertie en emprunt",
            Message = $"Votre reservation pour \"{livre.Titre}\" a ete convertie en emprunt. Retour prevu le {dateRetourPrevu:dd/MM/yyyy}.",
            Type = "emprunt_confirme",
            EstLue = false,
            DateCreation = DateTime.Now,
            Lien = "/Emprunt/MesEmprunts"
        });

        await _context.SaveChangesAsync();

        string nomMembre = $"{reservation.IdMembreNavigation?.Prenom} {reservation.IdMembreNavigation?.Nom}".Trim();
        TempData["Success"] = $"Reservation de {nomMembre} convertie en emprunt. Retour prevu le {dateRetourPrevu:dd/MM/yyyy}.";
        return RedirectToAction(nameof(Index));
    }
}
