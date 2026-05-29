using GestionBibliotheque.Constants;
using GestionBibliotheque.Data;
using GestionBibliotheque.Models;
using GestionBibliotheque.Services;
using GestionBibliotheque.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Controllers;

public class AuthController : Controller
{
    private readonly BibliothequeDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AuthController(BibliothequeDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    public IActionResult Inscription()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscription(InscriptionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        bool loginExiste = await _context.Utilisateurs
            .AnyAsync(u => u.Login == model.Login);
        if (loginExiste)
        {
            ModelState.AddModelError("Login", "Cet identifiant est deja utilise.");
            return View(model);
        }

        bool mailExiste = await _context.Membres
            .AnyAsync(m => m.Mail == model.Mail);
        if (mailExiste)
        {
            ModelState.AddModelError("Mail", "Cette adresse email est deja utilisee.");
            return View(model);
        }

        try
        {
            var membre = new Membre
            {
                Nom = model.Nom,
                Prenom = model.Prenom,
                Sexe = model.Sexe,
                Telephone = model.Telephone,
                Mail = model.Mail,
                Adresse = model.Adresse,
                DateInscription = DateOnly.FromDateTime(DateTime.Today),
                Statut = AbonnementStatuses.Actif
            };

            _context.Membres.Add(membre);
            await _context.SaveChangesAsync();

            var utilisateur = new Utilisateur
            {
                Login = model.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = ApplicationRoles.Membre,
                IdMembre = membre.IdMembre
            };

            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Compte cree avec succes. Vous pouvez maintenant vous connecter.";
            return RedirectToAction("Login", "Auth");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Une erreur s'est produite lors de la creation de votre compte. Veuillez reessayer.";
            System.Diagnostics.Debug.WriteLine($"Erreur inscription: {ex.Message}");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.IdMembreNavigation)
                .FirstOrDefaultAsync(u => u.Login == model.Login);

            if (utilisateur == null || !BCrypt.Net.BCrypt.Verify(model.Password, utilisateur.Password))
            {
                TempData["Error"] = "Identifiant ou mot de passe incorrect.";
                return View(model);
            }

            string nomComplet;
            int? membreId = null;

            if (utilisateur.IdMembreNavigation != null)
            {
                membreId = utilisateur.IdMembreNavigation.IdMembre;
                nomComplet = $"{utilisateur.IdMembreNavigation.Prenom} {utilisateur.IdMembreNavigation.Nom}".Trim();
            }
            else
            {
                nomComplet = utilisateur.Login ?? "Utilisateur";
            }

            _currentUser.SignIn(
                utilisateur.IdUtilisateur,
                utilisateur.Role ?? ApplicationRoles.Membre,
                utilisateur.Login ?? string.Empty,
                membreId,
                nomComplet);

            TempData["Success"] = $"Bienvenue {utilisateur.IdMembreNavigation?.Prenom ?? utilisateur.Login} ! Connexion reussie.";

            if (utilisateur.Role == ApplicationRoles.Admin)
            {
                return RedirectToAction("Index", "AdminDashboard");
            }

            return RedirectToAction("Index", "MemberDashboard");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Une erreur s'est produite lors de la connexion. Veuillez reessayer.";
            System.Diagnostics.Debug.WriteLine($"Erreur login: {ex.Message}");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Bienvenue()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        string userName = _currentUser.DisplayName;

        _currentUser.SignOut();

        TempData["Success"] = $"Au revoir {userName} ! Vous avez ete deconnecte avec succes.";
        return RedirectToAction("Index", "Home");
    }
}
