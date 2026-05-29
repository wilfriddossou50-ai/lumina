using GestionBibliotheque.Constants;
using GestionBibliotheque.Data;
using GestionBibliotheque.Models;
using GestionBibliotheque.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Controllers;

public class MemberDashboardController : Controller
{
    private readonly BibliothequeDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILayoutService _layoutService;

    public MemberDashboardController(
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
                     && (!a.DateFin.HasValue || a.DateFin.Value >= today))
            .OrderByDescending(a => a.IdAbonnment)
            .FirstOrDefaultAsync();
    }

    public async Task<IActionResult> Index()
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            TempData["Error"] = "Veuillez vous connecter pour acceder a votre tableau de bord.";
            return RedirectToAction("Login", "Auth");
        }

        int membreId = _currentUser.MembreId.Value;
        string membreNom = _currentUser.DisplayName;
        string prenom = membreNom.Split(' ').FirstOrDefault() ?? membreNom;

        await _layoutService.PopulateAsync(this, "Dashboard");
        ViewBag.Prenom = prenom;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var abonnement = await GetAbonnementActifAsync(membreId);

        var limites = PlanLimitesConfiguration.GetForTarif(abonnement?.IdTypeNavigation?.Tarif);
        ViewBag.AbonnementLibelle = abonnement?.IdTypeNavigation?.Libelle ?? "Gratuit";
        ViewBag.DateExpiration = abonnement?.DateFin?.ToString("dd MMM yyyy");
        ViewBag.MaxEmprunts = limites.MaxEmpruntsSimultanes;
        ViewBag.MaxReservations = limites.MaxReservationsActives;
        ViewBag.MaxAlertes = limites.MaxAlertesActives;
        ViewBag.DureeEmprunt = limites.DureeEmpruntJours;

        var empruntsEnCours = await _context.Emprunts
            .Include(e => e.DetailEmprunts)
                .ThenInclude(d => d.IdLivreNavigation)
            .Where(e => e.IdMembre == membreId && e.Statut == EmpruntStatuses.EnCours)
            .OrderBy(e => e.DateEmprunt)
            .Take(5)
            .ToListAsync();

        var finSemaine = today.AddDays(7);

        int retoursASemaine = await _context.DetailEmprunts
            .Where(d => d.IdEmpruntNavigation.IdMembre == membreId
                     && d.IdEmpruntNavigation.Statut == EmpruntStatuses.EnCours
                     && d.DateRetourPrevu.HasValue
                     && d.DateRetourPrevu.Value <= finSemaine)
            .CountAsync();

        var reservations = await _context.Reservations
            .Include(r => r.IdLivreNavigation)
            .Where(r => r.IdMembre == membreId && r.Statut == ReservationStatuses.EnAttente)
            .OrderBy(r => r.DateReservation)
            .Take(3)
            .ToListAsync();

        int nbAlertesActives = await _context.Alertes
            .Where(a => a.IdMembre == membreId && a.EstTraitee == false)
            .CountAsync();

        var livresEmpruntes = await _context.Emprunts
            .Where(e => e.IdMembre == membreId && e.Statut == EmpruntStatuses.EnCours)
            .SelectMany(e => e.DetailEmprunts)
            .Select(d => d.IdLivre)
            .Distinct()
            .ToListAsync();

        var livresReserves = await _context.Reservations
            .Where(r => r.IdMembre == membreId && r.Statut == ReservationStatuses.EnAttente && r.IdLivre.HasValue)
            .Select(r => r.IdLivre)
            .Distinct()
            .ToListAsync();

        var livresAlertes = await _context.Alertes
            .Where(a => a.IdMembre == membreId && a.EstTraitee == false)
            .Select(a => a.IdLivre)
            .Distinct()
            .ToListAsync();

        var idLivres = reservations
            .Where(r => r.IdLivre.HasValue)
            .Select(r => r.IdLivre!.Value)
            .Distinct()
            .ToList();

        var reservationsGroupees = await _context.Reservations
            .Where(r => idLivres.Contains(r.IdLivre!.Value) && r.Statut == ReservationStatuses.EnAttente)
            .Select(r => new { r.IdLivre, r.IdReservation, r.DateReservation })
            .ToListAsync();

        var positionsAttente = new Dictionary<int, int>();
        foreach (var reservation in reservations.Where(r => r.IdLivre.HasValue))
        {
            int position = reservationsGroupees
                .Count(r => r.IdLivre == reservation.IdLivre
                         && r.DateReservation < reservation.DateReservation) + 1;
            positionsAttente[reservation.IdReservation] = position;
        }

        var derniersLivres = await _context.DetailEmprunts
            .Include(d => d.IdLivreNavigation)
                .ThenInclude(l => l.IdCategorieNavigation)
            .Include(d => d.IdEmpruntNavigation)
            .Where(d => d.IdEmpruntNavigation.IdMembre == membreId
                     && d.IdLivreNavigation != null)
            .OrderByDescending(d => d.IdEmpruntNavigation.DateEmprunt)
            .Select(d => d.IdLivreNavigation!)
            .Distinct()
            .Take(8)
            .ToListAsync();

        ViewBag.RetoursASemaine = retoursASemaine;
        ViewBag.EmpruntsEnCours = empruntsEnCours;
        ViewBag.Reservations = reservations;
        ViewBag.PositionsAttente = positionsAttente;
        ViewBag.DerniersLivres = derniersLivres;
        ViewBag.LivresEmpruntes = livresEmpruntes;
        ViewBag.LivresReserves = livresReserves;
        ViewBag.LivresAlertes = livresAlertes;
        ViewBag.NbEmpruntsActifs = empruntsEnCours.Count;
        ViewBag.NbReservationsActives = reservations.Count;
        ViewBag.NbAlertesActives = nbAlertesActives;

        return View();
    }
}
