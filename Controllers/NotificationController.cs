using GestionBibliotheque.Data;
using GestionBibliotheque.Models;
using GestionBibliotheque.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Controllers;

public class NotificationController : Controller
{
    private readonly BibliothequeDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILayoutService _layoutService;

    public NotificationController(
        BibliothequeDbContext context,
        ICurrentUserService currentUser,
        ILayoutService layoutService)
    {
        _context = context;
        _currentUser = currentUser;
        _layoutService = layoutService;
    }

    private IQueryable<Notification> QueryNotificationsUtilisateur()
    {
        if (_currentUser.IsAdmin && _currentUser.UtilisateurId.HasValue)
        {
            return _context.Notifications.Where(n => n.IdUtilisateur == _currentUser.UtilisateurId.Value);
        }

        if (_currentUser.IsMembre && _currentUser.MembreId.HasValue)
        {
            return _context.Notifications.Where(n => n.IdMembre == _currentUser.MembreId.Value);
        }

        return _context.Notifications.Where(_ => false);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!_currentUser.IsAuthenticated)
        {
            TempData["Error"] = "Veuillez vous connecter pour acceder aux notifications.";
            return RedirectToAction("Login", "Auth");
        }

        await _layoutService.PopulateAsync(this, "Notifications");

        var notifications = await QueryNotificationsUtilisateur()
            .OrderByDescending(n => n.DateCreation)
            .ToListAsync();

        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return RedirectToAction("Login", "Auth");
        }

        var notification = await QueryNotificationsUtilisateur()
            .FirstOrDefaultAsync(n => n.IdNotification == id);

        if (notification == null)
        {
            TempData["Error"] = "Notification introuvable ou acces refuse.";
            return RedirectToAction(nameof(Index));
        }

        notification.EstLue = true;
        notification.DateLue = DateTime.Now;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        if (!_currentUser.IsAuthenticated)
        {
            return RedirectToAction("Login", "Auth");
        }

        var notifs = await QueryNotificationsUtilisateur()
            .Where(n => n.EstLue == false)
            .ToListAsync();

        foreach (var notif in notifs)
        {
            notif.EstLue = true;
            notif.DateLue = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Toutes les notifications ont ete marquees comme lues.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return RedirectToAction("Login", "Auth");
        }

        var notification = await QueryNotificationsUtilisateur()
            .FirstOrDefaultAsync(n => n.IdNotification == id);

        if (notification == null)
        {
            TempData["Error"] = "Notification introuvable ou acces refuse.";
            return RedirectToAction(nameof(Index));
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Notification supprimee.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAll()
    {
        if (!_currentUser.IsAuthenticated)
        {
            return RedirectToAction("Login", "Auth");
        }

        var notifications = await QueryNotificationsUtilisateur().ToListAsync();

        if (notifications.Count == 0)
        {
            TempData["Warning"] = "Aucune notification a supprimer.";
            return RedirectToAction(nameof(Index));
        }

        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Toutes les notifications ont ete supprimees.";
        return RedirectToAction(nameof(Index));
    }
}
