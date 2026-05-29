using GestionBibliotheque.Data;
using GestionBibliotheque.Models;
using GestionBibliotheque.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Controllers;

public class AlerteController : Controller
{
    private readonly BibliothequeDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILayoutService _layoutService;

    public AlerteController(
        BibliothequeDbContext context,
        ICurrentUserService currentUser,
        ILayoutService layoutService)
    {
        _context = context;
        _currentUser = currentUser;
        _layoutService = layoutService;
    }

    [HttpGet]
    public async Task<IActionResult> MesAlertes()
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Login", "Auth");
        }

        await _layoutService.PopulateAsync(this, "Alertes");

        int membreId = _currentUser.MembreId.Value;
        var alertes = await _context.Alertes
            .Include(a => a.IdLivreNavigation)
            .Where(a => a.IdMembre == membreId)
            .OrderByDescending(a => a.DateDemande)
            .ToListAsync();

        return View(alertes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Supprimer(int id)
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Login", "Auth");
        }

        var alerte = await _context.Alertes.FindAsync(id);
        if (alerte == null || alerte.IdMembre != _currentUser.MembreId.Value)
        {
            TempData["Error"] = "Alerte introuvable ou acces refuse.";
            return RedirectToAction(nameof(MesAlertes));
        }

        _context.Alertes.Remove(alerte);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Alerte supprimee.";
        return RedirectToAction(nameof(MesAlertes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SupprimerToutes()
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Login", "Auth");
        }

        int membreId = _currentUser.MembreId.Value;
        var alertes = await _context.Alertes
            .Where(a => a.IdMembre == membreId)
            .ToListAsync();

        if (alertes.Count == 0)
        {
            TempData["Warning"] = "Aucune alerte a supprimer.";
            return RedirectToAction(nameof(MesAlertes));
        }

        _context.Alertes.RemoveRange(alertes);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Toutes vos alertes ont ete supprimees.";
        return RedirectToAction(nameof(MesAlertes));
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        await _layoutService.PopulateAsync(this, "Alertes");

        var alertes = await _context.Alertes
            .Include(a => a.IdLivreNavigation)
            .Include(a => a.IdMembreNavigation)
            .OrderByDescending(a => a.DateDemande)
            .ToListAsync();

        return View(alertes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Traiter(int id)
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        var alerte = await _context.Alertes
            .Include(a => a.IdLivreNavigation)
            .FirstOrDefaultAsync(a => a.IdAlerte == id);

        if (alerte == null)
        {
            TempData["Error"] = "Alerte introuvable.";
            return RedirectToAction(nameof(Index));
        }

        if (alerte.EstTraitee == true)
        {
            TempData["Warning"] = "Cette alerte a deja ete traitee.";
            return RedirectToAction(nameof(Index));
        }

        alerte.EstTraitee = true;
        alerte.DateNotification = DateOnly.FromDateTime(DateTime.Today);

        var livreTitre = alerte.IdLivreNavigation?.Titre ?? "ce livre";

        _context.Notifications.Add(new Notification
        {
            IdMembre = alerte.IdMembre,
            Titre = "Alerte traitee",
            Message = $"L'alerte pour \"{livreTitre}\" a ete traitee. Nous vous informerons des qu'il sera disponible.",
            Type = "alerte_traitee",
            EstLue = false,
            Lien = $"/Catalogue/Details/{alerte.IdLivre}",
            DateCreation = DateTime.Now
        });

        await _context.SaveChangesAsync();

        TempData["Success"] = "Alerte marquee comme traitee et membre notifie.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SupprimerAdmin(int id)
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        var alerte = await _context.Alertes.FindAsync(id);
        if (alerte == null)
        {
            TempData["Error"] = "Alerte introuvable.";
            return RedirectToAction(nameof(Index));
        }

        _context.Alertes.Remove(alerte);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Alerte supprimee.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SupprimerToutesAdmin()
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        var alertes = await _context.Alertes.ToListAsync();
        if (alertes.Count == 0)
        {
            TempData["Warning"] = "Aucune alerte a supprimer.";
            return RedirectToAction(nameof(Index));
        }

        _context.Alertes.RemoveRange(alertes);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Toutes les alertes ont ete supprimees.";
        return RedirectToAction(nameof(Index));
    }
}
