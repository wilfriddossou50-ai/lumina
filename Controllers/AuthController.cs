using GestionBibliotheque.Data;
using GestionBibliotheque.Models;
using GestionBibliotheque.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Controllers;

public class AuthController : Controller
{
    private readonly BibliothequeDbContext _context;

    public AuthController(BibliothequeDbContext context)
    {
        _context = context;
    }

    // GET: /Auth/Inscription
    [HttpGet]
    public IActionResult Inscription()
    {
        return View();
    }

    // POST: /Auth/Inscription
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscription(InscriptionViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Vérifier si le login existe déjà
        bool loginExiste = await _context.Utilisateurs
            .AnyAsync(u => u.Login == model.Login);
        if (loginExiste)
        {
            ModelState.AddModelError("Login", "Cet identifiant est déjà utilisé.");
            return View(model);
        }

        // Vérifier si le mail existe déjà
        bool mailExiste = await _context.Membres
            .AnyAsync(m => m.Mail == model.Mail);
        if (mailExiste)
        {
            ModelState.AddModelError("Mail", "Cette adresse email est déjà utilisée.");
            return View(model);
        }

        // 1. Créer le Membre
        var membre = new Membre
        {
            Nom      = model.Nom,
            Prenom   = model.Prenom,
            Sexe     = model.Sexe,
            Telephone = model.Telephone,
            Mail     = model.Mail,
            Adresse  = model.Adresse,
            DateInscription = DateOnly.FromDateTime(DateTime.Today),
            Statut   = "actif"
        };

        _context.Membres.Add(membre);
        await _context.SaveChangesAsync(); // Pour obtenir l'ID généré

        // 2. Créer l'Utilisateur lié
        var utilisateur = new Utilisateur
        {
            Login    = model.Login,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Role     = "membre",
            IdMembre = membre.IdMembre
        };

        _context.Utilisateurs.Add(utilisateur);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Compte créé avec succès ! Vous pouvez maintenant vous connecter.";
        return RedirectToAction("Login", "Inscription");
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
}
