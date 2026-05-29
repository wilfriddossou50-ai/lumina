using GestionBibliotheque.Constants;
using GestionBibliotheque.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Services;

public class LayoutService : ILayoutService
{
    private readonly BibliothequeDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public LayoutService(BibliothequeDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task PopulateAsync(Controller controller, string activePage)
    {
        controller.ViewData["ActivePage"] = activePage;
        controller.ViewBag.MembreNom = _currentUser.DisplayName;
        controller.ViewBag.Initiales = _currentUser.Initials;
        controller.ViewBag.CurrentRole = _currentUser.Role;

        var notificationCount = await GetUnreadNotificationCountAsync();

        controller.ViewBag.NotificationCount = notificationCount;

        if (_currentUser.IsAdmin)
        {
            controller.ViewBag.NbNotifsAdmin = notificationCount;
        }
        else if (_currentUser.IsMembre)
        {
            controller.ViewBag.NbNotifs = notificationCount;
        }
    }

    private Task<int> GetUnreadNotificationCountAsync()
    {
        if (_currentUser.IsAdmin && _currentUser.UtilisateurId.HasValue)
        {
            return _context.Notifications.CountAsync(n =>
                n.IdUtilisateur == _currentUser.UtilisateurId.Value &&
                n.EstLue == false);
        }

        if (_currentUser.IsMembre && _currentUser.MembreId.HasValue)
        {
            return _context.Notifications.CountAsync(n =>
                n.IdMembre == _currentUser.MembreId.Value &&
                n.EstLue == false);
        }

        return Task.FromResult(0);
    }
}
