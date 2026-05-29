using GestionBibliotheque.Constants;
using GestionBibliotheque.Data;
using GestionBibliotheque.Services;
using GestionBibliotheque.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Controllers;

public class ProfilController : Controller
{
    private readonly BibliothequeDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILayoutService _layoutService;

    public ProfilController(
        BibliothequeDbContext context,
        ICurrentUserService currentUser,
        ILayoutService layoutService)
    {
        _context = context;
        _currentUser = currentUser;
        _layoutService = layoutService;
    }

    private static string GetInitiales(string nom)
    {
        if (string.IsNullOrWhiteSpace(nom))
        {
            return "?";
        }

        var parts = nom.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "?";
        }

        return parts.Length == 1
            ? parts[0][0].ToString().ToUpperInvariant()
            : string.Concat(parts[0][0], parts[1][0]).ToUpperInvariant();
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!_currentUser.IsAuthenticated)
        {
            TempData["Error"] = "Veuillez vous connecter pour acceder a votre profil.";
            return RedirectToAction("Login", "Auth");
        }

        await _layoutService.PopulateAsync(this, "Profil");

        var page = await BuildProfilePageAsync();
        if (page == null)
        {
            TempData["Error"] = "Utilisateur introuvable.";
            return RedirectToAction("Login", "Auth");
        }

        return View(page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfilPageViewModel pageModel)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UtilisateurId.HasValue)
        {
            TempData["Error"] = "Veuillez vous connecter pour modifier votre profil.";
            return RedirectToAction("Login", "Auth");
        }

        var model = pageModel.Profile;

        ModelState.Clear();
        if (!TryValidateModel(model, nameof(ProfilPageViewModel.Profile)))
        {
            await _layoutService.PopulateAsync(this, "Profil");
            return View("Index", await BuildProfilePageAsync(profileOverride: model));
        }

        var utilisateur = await _context.Utilisateurs
            .Include(u => u.IdMembreNavigation)
            .FirstOrDefaultAsync(u => u.IdUtilisateur == _currentUser.UtilisateurId.Value);

        if (utilisateur == null)
        {
            TempData["Error"] = "Utilisateur introuvable.";
            return RedirectToAction(nameof(Index));
        }

        bool loginExiste = await _context.Utilisateurs
            .AnyAsync(u => u.Login == model.Login && u.IdUtilisateur != utilisateur.IdUtilisateur);

        if (loginExiste)
        {
            ModelState.AddModelError("Profile.Login", "Ce login est deja utilise.");
            await _layoutService.PopulateAsync(this, "Profil");
            return View("Index", await BuildProfilePageAsync(profileOverride: model));
        }

        utilisateur.Login = model.Login;

        string displayName = utilisateur.Login ?? "Utilisateur";

        if (utilisateur.IdMembreNavigation != null && model.IdMembre.HasValue)
        {
            var membre = utilisateur.IdMembreNavigation;
            bool mailExiste = await _context.Membres
                .AnyAsync(m => m.Mail == model.Mail && m.IdMembre != membre.IdMembre);

            if (mailExiste)
            {
                ModelState.AddModelError("Profile.Mail", "Cette adresse email est deja utilisee.");
                await _layoutService.PopulateAsync(this, "Profil");
                return View("Index", await BuildProfilePageAsync(profileOverride: model));
            }

            membre.Nom = model.Nom;
            membre.Prenom = model.Prenom;
            membre.Sexe = model.Sexe;
            membre.Telephone = model.Telephone;
            membre.Mail = model.Mail;
            membre.Adresse = model.Adresse;

            displayName = string.IsNullOrWhiteSpace(model.DisplayName) ? model.Login : model.DisplayName;
        }

        await _context.SaveChangesAsync();

        _currentUser.SignIn(
            utilisateur.IdUtilisateur,
            utilisateur.Role ?? ApplicationRoles.Membre,
            utilisateur.Login ?? string.Empty,
            utilisateur.IdMembre,
            displayName);

        TempData["Success"] = "Votre profil a bien ete mis a jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ProfilPageViewModel pageModel)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UtilisateurId.HasValue)
        {
            TempData["Error"] = "Veuillez vous connecter pour modifier votre mot de passe.";
            return RedirectToAction("Login", "Auth");
        }

        var model = pageModel.Password;

        ModelState.Clear();
        if (!TryValidateModel(model, nameof(ProfilPageViewModel.Password)))
        {
            await _layoutService.PopulateAsync(this, "Profil");
            return View("Index", await BuildProfilePageAsync(passwordOverride: model));
        }

        var utilisateur = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.IdUtilisateur == _currentUser.UtilisateurId.Value);

        if (utilisateur == null)
        {
            TempData["Error"] = "Utilisateur introuvable.";
            return RedirectToAction("Login", "Auth");
        }

        if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, utilisateur.Password))
        {
            ModelState.AddModelError("Password.CurrentPassword", "Mot de passe actuel incorrect.");
            await _layoutService.PopulateAsync(this, "Profil");
            return View("Index", await BuildProfilePageAsync(passwordOverride: model));
        }

        utilisateur.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Votre mot de passe a bien ete change.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ProfilPageViewModel?> BuildProfilePageAsync(
        ProfilViewModel? profileOverride = null,
        ChangePasswordViewModel? passwordOverride = null)
    {
        if (!_currentUser.UtilisateurId.HasValue)
        {
            return null;
        }

        var utilisateur = await _context.Utilisateurs
            .Include(u => u.IdMembreNavigation)
            .FirstOrDefaultAsync(u => u.IdUtilisateur == _currentUser.UtilisateurId.Value);

        if (utilisateur == null)
        {
            return null;
        }

        var profile = profileOverride ?? new ProfilViewModel
        {
            IdUtilisateur = utilisateur.IdUtilisateur,
            IdMembre = utilisateur.IdMembre,
            Login = utilisateur.Login ?? string.Empty,
            Role = utilisateur.Role ?? string.Empty,
            Nom = utilisateur.IdMembreNavigation?.Nom,
            Prenom = utilisateur.IdMembreNavigation?.Prenom,
            Sexe = utilisateur.IdMembreNavigation?.Sexe,
            Telephone = utilisateur.IdMembreNavigation?.Telephone,
            Mail = utilisateur.IdMembreNavigation?.Mail,
            Adresse = utilisateur.IdMembreNavigation?.Adresse,
            Statut = utilisateur.IdMembreNavigation?.Statut,
            DateInscription = utilisateur.IdMembreNavigation?.DateInscription
        };

        bool isAdmin = _currentUser.IsAdmin;
        string displayName = string.IsNullOrWhiteSpace(profile.DisplayName)
            ? (profile.Login ?? "Utilisateur")
            : profile.DisplayName;

        return new ProfilPageViewModel
        {
            Profile = profile,
            Password = passwordOverride ?? new ChangePasswordViewModel(),
            IsAdmin = isAdmin,
            DashboardController = isAdmin ? "AdminDashboard" : "MemberDashboard",
            DashboardLabel = isAdmin ? "Retour au dashboard admin" : "Retour au tableau de bord",
            DisplayName = displayName,
            Initials = GetInitiales(displayName)
        };
    }
}
