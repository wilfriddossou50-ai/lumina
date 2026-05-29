using GestionBibliotheque.Constants;
using GestionBibliotheque.Data;
using GestionBibliotheque.Services;
using GestionBibliotheque.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Controllers;

public class AdminDashboardController : Controller
{
    private readonly BibliothequeDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILayoutService _layoutService;

    public AdminDashboardController(
        BibliothequeDbContext context,
        ICurrentUserService currentUser,
        ILayoutService layoutService)
    {
        _context = context;
        _currentUser = currentUser;
        _layoutService = layoutService;
    }

    public async Task<IActionResult> Index()
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        await _layoutService.PopulateAsync(this, "Dashboard");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var startOfMonth = new DateOnly(today.Year, today.Month, 1);
        var startOfWeek = today.AddDays(-6);
        var endOfWeek = today.AddDays(6);

        int totalLivres = await _context.Livres.CountAsync();
        int totalExemplaires = (int)await _context.Livres.SumAsync(l => l.QuantiteTotal ?? 0);
        int livresDisponibles = (int)await _context.Livres.SumAsync(l => l.QuantiteDisponible ?? 0);
        int totalMembres = await _context.Membres.CountAsync();
        int totalEmpruntsEnCours = await _context.Emprunts.CountAsync(e => e.Statut == EmpruntStatuses.EnCours);
        int totalRetards = await _context.DetailEmprunts.CountAsync(d =>
            d.DateRetourReel == null &&
            d.DateRetourPrevu.HasValue &&
            d.DateRetourPrevu.Value < today);
        int livresIndisponibles = await _context.Livres.CountAsync(l => (l.QuantiteDisponible ?? 0) == 0);
        int totalReservations = await _context.Reservations.CountAsync(r => r.Statut == ReservationStatuses.EnAttente);
        int nouveauxMembresCeMois = await _context.Membres.CountAsync(m =>
            m.DateInscription.HasValue && m.DateInscription.Value >= startOfMonth);
        int empruntsCetteSemaine = await _context.Emprunts.CountAsync(e =>
            e.DateEmprunt.HasValue && e.DateEmprunt.Value >= startOfWeek);
        int retoursPrevusCetteSemaine = await _context.DetailEmprunts.CountAsync(d =>
            d.DateRetourReel == null &&
            d.DateRetourPrevu.HasValue &&
            d.DateRetourPrevu.Value >= today &&
            d.DateRetourPrevu.Value <= endOfWeek);

        int tauxDisponibilite = totalExemplaires == 0
            ? 0
            : (int)Math.Round((double)livresDisponibles / totalExemplaires * 100);
        int tauxOccupation = totalExemplaires == 0
            ? 0
            : (int)Math.Round((double)(totalExemplaires - livresDisponibles) / totalExemplaires * 100);
        int tauxRetard = totalEmpruntsEnCours == 0
            ? 0
            : (int)Math.Round((double)totalRetards / totalEmpruntsEnCours * 100);
        int tauxReservation = totalLivres == 0
            ? 0
            : (int)Math.Round((double)totalReservations / totalLivres * 100);

        string niveauService = tauxRetard >= 25
            ? "Sous surveillance"
            : tauxDisponibilite >= 60
                ? "Tres bon"
                : "A renforcer";

        string messagePilotage = totalRetards > totalReservations
            ? "Les retards pesent davantage que les nouvelles reservations. Une relance ciblee ameliorerait rapidement la rotation."
            : "Le flux reste maitrise. Les reservations et la disponibilite du fonds sont globalement equilibrees.";

        var empruntsBruts = await _context.Emprunts
            .Where(e => e.DateEmprunt.HasValue && e.DateEmprunt.Value >= startOfWeek)
            .GroupBy(e => e.DateEmprunt!.Value)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var categoriesBrutes = await _context.Livres
            .Include(l => l.IdCategorieNavigation)
            .GroupBy(l => l.IdCategorieNavigation != null ? l.IdCategorieNavigation.Libelle : "Non classe")
            .Select(g => new
            {
                Label = g.Key ?? "Non classe",
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        int notificationCount = ViewBag.NotificationCount as int? ?? 0;

        var model = new AdminDashboardViewModel
        {
            TotalLivres = totalLivres,
            TotalExemplaires = totalExemplaires,
            TotalMembres = totalMembres,
            TotalEmpruntsEnCours = totalEmpruntsEnCours,
            TotalRetards = totalRetards,
            LivresIndisponibles = livresIndisponibles,
            TotalReservations = totalReservations,
            NbNotificationsNonLues = notificationCount,
            LivresDisponibles = livresDisponibles,
            NouveauxMembresCeMois = nouveauxMembresCeMois,
            EmpruntsCetteSemaine = empruntsCetteSemaine,
            RetoursPrevusCetteSemaine = retoursPrevusCetteSemaine,
            TauxDisponibilite = tauxDisponibilite,
            TauxOccupation = tauxOccupation,
            TauxRetard = tauxRetard,
            TauxReservation = tauxReservation,
            NiveauService = niveauService,
            MessagePilotage = messagePilotage,
            EmpruntsHebdomadaires = Enumerable.Range(0, 7)
                .Select(offset =>
                {
                    var date = startOfWeek.AddDays(offset);
                    var count = empruntsBruts.FirstOrDefault(x => x.Date == date)?.Count ?? 0;
                    return new DashboardTrendPointViewModel
                    {
                        Label = date.ToDateTime(TimeOnly.MinValue).ToString("ddd"),
                        Value = count
                    };
                })
                .ToList(),
            CategoriesPopulaires = categoriesBrutes
                .Select(c => new DashboardCategoryStatViewModel
                {
                    Label = c.Label,
                    Value = c.Count,
                    Percentage = totalLivres == 0 ? 0 : (int)Math.Round((double)c.Count / totalLivres * 100)
                })
                .ToList()
        };

        return View(model);
    }
}
