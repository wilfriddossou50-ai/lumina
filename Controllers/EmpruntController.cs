using GestionBibliotheque.Constants;
using GestionBibliotheque.Data;
using GestionBibliotheque.Models;
using GestionBibliotheque.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Controllers;

public class EmpruntController : Controller
{
    private readonly BibliothequeDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILayoutService _layoutService;

    public EmpruntController(
        BibliothequeDbContext context,
        ICurrentUserService currentUser,
        ILayoutService layoutService)
    {
        _context = context;
        _currentUser = currentUser;
        _layoutService = layoutService;
    }

    private void AjouterNotificationMembre(int membreId, string titre, string message, string type, string lien)
    {
        _context.Notifications.Add(new Notification
        {
            IdMembre = membreId,
            Titre = titre,
            Message = message,
            Type = type,
            Lien = lien,
            EstLue = false,
            DateCreation = DateTime.Now
        });
    }

    private async Task PromouvoirReservationDisponibleAsync(int idLivre, string titreLivre)
    {
        var reservation = await _context.Reservations
            .Include(r => r.IdMembreNavigation)
            .Where(r => r.IdLivre == idLivre && r.Statut == ReservationStatuses.EnAttente)
            .OrderBy(r => r.DateReservation)
            .ThenBy(r => r.IdReservation)
            .FirstOrDefaultAsync();

        if (reservation == null)
        {
            return;
        }

        reservation.Statut = ReservationStatuses.Disponible;

        AjouterNotificationMembre(
            reservation.IdMembre,
            "Livre reserve disponible",
            $"Le livre \"{titreLivre}\" est maintenant disponible pour vous.",
            "reservation_disponible",
            "/Reservation/MesReservations");
    }

    private async Task TraiterAlertesDisponibiliteAsync(int idLivre, string titreLivre)
    {
        var alertes = await _context.Alertes
            .Where(a => a.IdLivre == idLivre && a.EstTraitee == false)
            .OrderBy(a => a.DateDemande)
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        foreach (var alerte in alertes)
        {
            alerte.EstTraitee = true;
            alerte.DateNotification = today;

            AjouterNotificationMembre(
                alerte.IdMembre,
                "Livre disponible",
                $"Le livre \"{titreLivre}\" est de nouveau disponible.",
                "livre_disponible",
                $"/Catalogue/Details/{idLivre}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> MesEmprunts()
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Login", "Auth");
        }

        await _layoutService.PopulateAsync(this, "Emprunts");

        int membreId = _currentUser.MembreId.Value;
        var emprunts = await _context.Emprunts
            .Include(e => e.DetailEmprunts)
                .ThenInclude(d => d.IdLivreNavigation)
            .Where(e => e.IdMembre == membreId && e.Statut == EmpruntStatuses.EnCours)
            .OrderBy(e => e.DateEmprunt)
            .ToListAsync();

        return View(emprunts);
    }

    [HttpGet]
    public async Task<IActionResult> Historique()
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Login", "Auth");
        }

        await _layoutService.PopulateAsync(this, "Historique");

        int membreId = _currentUser.MembreId.Value;
        var emprunts = await _context.Emprunts
            .Include(e => e.DetailEmprunts)
                .ThenInclude(d => d.IdLivreNavigation)
            .Where(e => e.IdMembre == membreId)
            .OrderByDescending(e => e.DateEmprunt)
            .ToListAsync();

        return View(emprunts);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        await _layoutService.PopulateAsync(this, "Emprunts");

        var emprunts = await _context.Emprunts
            .Include(e => e.IdMembreNavigation)
            .Include(e => e.DetailEmprunts)
                .ThenInclude(d => d.IdLivreNavigation)
            .Where(e => e.Statut == EmpruntStatuses.EnCours)
            .OrderBy(e => e.DateEmprunt)
            .ToListAsync();

        return View(emprunts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderRetour(int id)
    {
        if (!_currentUser.IsAdmin || !_currentUser.UtilisateurId.HasValue)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        var emprunt = await _context.Emprunts
            .Include(e => e.DetailEmprunts)
            .ThenInclude(d => d.IdLivreNavigation)
            .FirstOrDefaultAsync(e => e.IdEmprunt == id);

        if (emprunt == null)
        {
            TempData["Error"] = "Emprunt introuvable.";
            return RedirectToAction(nameof(Index));
        }

        if (emprunt.Statut != EmpruntStatuses.EnCours)
        {
            TempData["Warning"] = "Cet emprunt a deja ete traite.";
            return RedirectToAction(nameof(Index));
        }

        emprunt.Statut = "retourne";
        emprunt.IdUserRetour = _currentUser.UtilisateurId.Value;

        foreach (var detail in emprunt.DetailEmprunts)
        {
            if (detail.DateRetourReel == null)
            {
                detail.DateRetourReel = DateOnly.FromDateTime(DateTime.Today);
            }

            var livre = await _context.Livres.FindAsync(detail.IdLivre);
            if (livre == null)
            {
                continue;
            }

            livre.QuantiteDisponible = (livre.QuantiteDisponible ?? 0) + 1;
            if (livre.QuantiteDisponible > 0 && livre.Statut == LivreStatuses.Indisponible)
            {
                livre.Statut = LivreStatuses.Disponible;
            }

            string titreLivre = livre.Titre ?? "ce livre";
            await PromouvoirReservationDisponibleAsync(detail.IdLivre, titreLivre);
            await TraiterAlertesDisponibiliteAsync(detail.IdLivre, titreLivre);
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Retour valide et livre(s) remis a jour.";
        return RedirectToAction(nameof(Index));
    }
}
