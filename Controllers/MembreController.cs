using GestionBibliotheque.Constants;
using GestionBibliotheque.Data;
using GestionBibliotheque.Models;
using GestionBibliotheque.Services;
using GestionBibliotheque.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Controllers;

public class MembreController : Controller
{
    private readonly BibliothequeDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILayoutService _layoutService;

    public MembreController(
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

        await _layoutService.PopulateAsync(this, "Membres");

        var membres = await _context.Membres
            .Include(m => m.Abonnements)
            .Include(m => m.Emprunts)
            .Include(m => m.Reservations)
            .OrderBy(m => m.Nom)
            .ThenBy(m => m.Prenom)
            .ToListAsync();

        return View(membres);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        if (id == null)
        {
            return NotFound();
        }

        await _layoutService.PopulateAsync(this, "Membres");

        var membre = await _context.Membres
            .Include(m => m.Abonnements)
                .ThenInclude(a => a.IdTypeNavigation)
            .Include(m => m.Emprunts)
                .ThenInclude(e => e.DetailEmprunts)
                    .ThenInclude(d => d.IdLivreNavigation)
            .Include(m => m.Reservations)
                .ThenInclude(r => r.IdLivreNavigation)
            .Include(m => m.Notifications)
            .Include(m => m.Alertes)
            .FirstOrDefaultAsync(m => m.IdMembre == id);

        if (membre == null)
        {
            return NotFound();
        }

        return View(membre);
    }

    public async Task<IActionResult> Create()
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        await _layoutService.PopulateAsync(this, "Membres");

        var model = new MembreAdminViewModel
        {
            Statut = AbonnementStatuses.Actif,
            AccepterConditions = true
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MembreAdminViewModel model)
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError("Password", "Le mot de passe est obligatoire.");
        }

        if (!ModelState.IsValid)
        {
            await _layoutService.PopulateAsync(this, "Membres");
            return View(model);
        }

        bool loginExiste = await _context.Utilisateurs
            .AnyAsync(u => u.Login == model.Login);
        if (loginExiste)
        {
            ModelState.AddModelError("Login", "Cet identifiant est deja utilise.");
            await _layoutService.PopulateAsync(this, "Membres");
            return View(model);
        }

        bool mailExiste = await _context.Membres
            .AnyAsync(m => m.Mail == model.Mail);
        if (mailExiste)
        {
            ModelState.AddModelError("Mail", "Cette adresse email est deja utilisee.");
            await _layoutService.PopulateAsync(this, "Membres");
            return View(model);
        }

        var membre = new Membre
        {
            Nom = model.Nom,
            Prenom = model.Prenom,
            Sexe = model.Sexe,
            Telephone = model.Telephone,
            Mail = model.Mail,
            Adresse = model.Adresse,
            DateInscription = DateOnly.FromDateTime(DateTime.Today),
            Statut = string.IsNullOrWhiteSpace(model.Statut) ? AbonnementStatuses.Actif : model.Statut
        };

        _context.Membres.Add(membre);
        await _context.SaveChangesAsync();

        var utilisateur = new Utilisateur
        {
            Login = model.Login,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password!),
            Role = ApplicationRoles.Membre,
            IdMembre = membre.IdMembre
        };

        _context.Utilisateurs.Add(utilisateur);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Membre cree avec succes.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        if (id == null)
        {
            return NotFound();
        }

        await _layoutService.PopulateAsync(this, "Membres");

        var membre = await _context.Membres
            .Include(m => m.Utilisateurs)
            .FirstOrDefaultAsync(m => m.IdMembre == id);
        if (membre == null)
        {
            return NotFound();
        }

        var utilisateur = membre.Utilisateurs.FirstOrDefault();
        var model = new MembreAdminViewModel
        {
            IdMembre = membre.IdMembre,
            IdUtilisateur = utilisateur?.IdUtilisateur,
            Nom = membre.Nom ?? string.Empty,
            Prenom = membre.Prenom ?? string.Empty,
            Sexe = membre.Sexe ?? string.Empty,
            Telephone = membre.Telephone,
            Mail = membre.Mail ?? string.Empty,
            Adresse = membre.Adresse,
            Statut = membre.Statut ?? AbonnementStatuses.Actif,
            Login = utilisateur?.Login ?? string.Empty,
            DateInscription = membre.DateInscription
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MembreAdminViewModel model)
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        if (id != model.IdMembre)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await _layoutService.PopulateAsync(this, "Membres");
            return View(model);
        }

        var membre = await _context.Membres.FindAsync(id);
        if (membre == null)
        {
            return NotFound();
        }

        var utilisateur = await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.IdMembre == id);

        int? utilisateurId = utilisateur?.IdUtilisateur;
        bool loginExiste = await _context.Utilisateurs
            .AnyAsync(u => u.Login == model.Login && u.IdUtilisateur != utilisateurId);
        if (loginExiste)
        {
            ModelState.AddModelError("Login", "Cet identifiant est deja utilise.");
            await _layoutService.PopulateAsync(this, "Membres");
            return View(model);
        }

        bool mailExiste = await _context.Membres
            .AnyAsync(m => m.Mail == model.Mail && m.IdMembre != id);
        if (mailExiste)
        {
            ModelState.AddModelError("Mail", "Cette adresse email est deja utilisee.");
            await _layoutService.PopulateAsync(this, "Membres");
            return View(model);
        }

        membre.Nom = model.Nom;
        membre.Prenom = model.Prenom;
        membre.Sexe = model.Sexe;
        membre.Telephone = model.Telephone;
        membre.Mail = model.Mail;
        membre.Adresse = model.Adresse;
        membre.Statut = model.Statut;
        membre.DateInscription = model.DateInscription ?? membre.DateInscription;

        if (utilisateur != null)
        {
            utilisateur.Login = model.Login;
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                utilisateur.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Un mot de passe est requis pour creer le compte utilisateur.");
                await _layoutService.PopulateAsync(this, "Membres");
                return View(model);
            }

            _context.Utilisateurs.Add(new Utilisateur
            {
                Login = model.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = ApplicationRoles.Membre,
                IdMembre = membre.IdMembre
            });
        }

        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Membre modifie avec succes.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MembreExists(membre.IdMembre))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        if (id == null)
        {
            return NotFound();
        }

        await _layoutService.PopulateAsync(this, "Membres");

        var membre = await _context.Membres
            .FirstOrDefaultAsync(m => m.IdMembre == id);

        if (membre == null)
        {
            return NotFound();
        }

        return View(membre);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        var membre = await _context.Membres
            .Include(m => m.Utilisateurs)
            .Include(m => m.Abonnements)
            .Include(m => m.Emprunts)
            .Include(m => m.Reservations)
            .Include(m => m.Notifications)
            .Include(m => m.Alertes)
            .FirstOrDefaultAsync(m => m.IdMembre == id);

        if (membre == null)
        {
            TempData["Warning"] = "Membre introuvable.";
            return RedirectToAction(nameof(Index));
        }

        bool hasBusinessData =
            membre.Abonnements.Count > 0 ||
            membre.Emprunts.Count > 0 ||
            membre.Reservations.Count > 0 ||
            membre.Alertes.Count > 0 ||
            membre.Notifications.Count > 0;

        if (hasBusinessData)
        {
            TempData["Warning"] = "Ce membre ne peut pas etre supprime car il possede deja un historique ou des donnees liees.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (membre.Utilisateurs.Count > 0)
        {
            _context.Utilisateurs.RemoveRange(membre.Utilisateurs);
        }

        _context.Membres.Remove(membre);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Membre supprime avec succes.";
        return RedirectToAction(nameof(Index));
    }

    private bool MembreExists(int id)
    {
        return _context.Membres.Any(e => e.IdMembre == id);
    }
}
