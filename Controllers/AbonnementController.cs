using GestionBibliotheque.Constants;
using GestionBibliotheque.Data;
using GestionBibliotheque.Models;
using GestionBibliotheque.Services;
using GestionBibliotheque.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Controllers;

public class AbonnementController : Controller
{
    private readonly BibliothequeDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILayoutService _layoutService;

    private const string PaiementMtnLabel = "MTN Money (+2290159703143)";
    private const string PaiementMoovLabel = "MOOV (bientot disponible)";
    private const string PaiementCeltisLabel = "CELTIS (0147837679)";

    public AbonnementController(
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
                     && (a.DateFin == null || a.DateFin.Value >= today))
            .OrderByDescending(a => a.IdAbonnment)
            .FirstOrDefaultAsync();
    }

    private async Task<Abonnement?> GetDemandeEnAttenteAsync(int membreId)
    {
        return await _context.Abonnements
            .Include(a => a.IdTypeNavigation)
            .Where(a => a.IdMembre == membreId
                     && a.StatutPaiement == PaiementStatuses.EnAttente)
            .OrderByDescending(a => a.IdAbonnment)
            .FirstOrDefaultAsync();
    }

    private async Task PreparerLayoutMembreAsync(string activePage)
    {
        await _layoutService.PopulateAsync(this, activePage);

        if (_currentUser.MembreId.HasValue)
        {
            var abonnement = await GetAbonnementActifAsync(_currentUser.MembreId.Value);
            ViewBag.AbonnementLibelle = abonnement?.IdTypeNavigation?.Libelle ?? "Gratuit";
        }
    }

    private static DateOnly? CalculerDateFin(float? montant)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        float montantNormalise = montant ?? 0f;

        if (montantNormalise <= 0f)
        {
            return null;
        }

        if (Math.Abs(montantNormalise - 5000f) < 0.5f)
        {
            return today.AddDays(365);
        }

        return today.AddDays(30);
    }

    [HttpGet]
    public async Task<IActionResult> Souscrire()
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            TempData["Error"] = "Veuillez vous connecter pour souscrire a un abonnement.";
            return RedirectToAction("Login", "Auth");
        }

        await PreparerLayoutMembreAsync("Abonnement");

        int membreId = _currentUser.MembreId.Value;
        var abonnementActif = await GetAbonnementActifAsync(membreId);
        var demandeEnAttente = await GetDemandeEnAttenteAsync(membreId);
        bool estActif = abonnementActif != null;
        bool enAttente = demandeEnAttente != null;

        var plans = await _context.TypeAbonnements
            .OrderBy(t => t.Tarif)
            .ToListAsync();

        var vm = new SouscriptionAbonnementViewModel
        {
            Plans = plans,
            AbonnementActuel = abonnementActif,
            // L'abonnement actif est le vrai "plan en cours".
            EstActif = estActif,
            EnAttente = enAttente,
            MethodesPaiement = new()
            {
                new("mtn", PaiementMtnLabel),
                new("moov", PaiementMoovLabel),
                new("celtis", PaiementCeltisLabel)
            }
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Souscrire(SouscrirePostViewModel model)
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            TempData["Error"] = "Veuillez vous connecter pour souscrire a un abonnement.";
            return RedirectToAction("Login", "Auth");
        }

        int membreId = _currentUser.MembreId.Value;

        var type = await _context.TypeAbonnements
            .FirstOrDefaultAsync(t => t.IdType == model.IdType);

        if (type == null)
        {
            TempData["Error"] = "Le plan selectionne est invalide.";
            return RedirectToAction(nameof(Souscrire));
        }

        var abonnementActif = await GetAbonnementActifAsync(membreId);
        if (abonnementActif != null)
        {
            TempData["Warning"] = "Vous avez deja un abonnement actif. Vous pouvez le consulter ou attendre son expiration avant d'en demander un autre.";
            return RedirectToAction(nameof(MonAbonnement));
        }

        bool enAttente = await _context.Abonnements
            .AnyAsync(a => a.IdMembre == membreId && a.StatutPaiement == PaiementStatuses.EnAttente);

        if (enAttente)
        {
            TempData["Warning"] = "Vous avez deja une demande en attente de validation.";
            return RedirectToAction(nameof(MonAbonnement));
        }

        var abonnement = new Abonnement
        {
            IdMembre = membreId,
            IdType = type.IdType,
            Montant = type.Tarif,
            DateDebut = null,
            DateFin = null,
            Statut = AbonnementStatuses.Inactif,
            ModePaiement = string.IsNullOrWhiteSpace(model.ModePaiement) ? "mtn" : model.ModePaiement.Trim().ToLowerInvariant(),
            ReferencePaiement = null,
            DatePaiement = null,
            StatutPaiement = PaiementStatuses.EnAttente,
            CodeReference = $"ABO-{Guid.NewGuid():N}".Substring(0, 12).ToUpperInvariant()
        };

        _context.Abonnements.Add(abonnement);
        await _context.SaveChangesAsync();

        _context.Notifications.Add(new Notification
        {
            IdMembre = membreId,
            IdUtilisateur = null,
            Titre = "Demande d'abonnement envoyee",
            Message = $"Votre demande pour le plan \"{type.Libelle}\" a ete envoyee. Elle sera activee apres validation de l'administrateur.",
            Type = "abonnement_demande",
            EstLue = false,
            DateCreation = DateTime.Now,
            Lien = "/Abonnement/MonAbonnement"
        });

        var adminIds = await _context.Utilisateurs
            .Where(u => u.Role == ApplicationRoles.Admin)
            .Select(u => u.IdUtilisateur)
            .ToListAsync();

        string membreNom = _currentUser.DisplayName;
        string mode = abonnement.ModePaiement switch
        {
            "mtn" => PaiementMtnLabel,
            "celtis" => PaiementCeltisLabel,
            "moov" => PaiementMoovLabel,
            _ => abonnement.ModePaiement ?? "-"
        };

        foreach (var adminId in adminIds)
        {
            _context.Notifications.Add(new Notification
            {
                IdMembre = null,
                IdUtilisateur = adminId,
                Titre = "Nouvelle demande d'abonnement",
                Message = $"{membreNom} a demande le plan \"{type.Libelle}\" ({(type.Tarif ?? 0):0} FCFA) via {mode}.",
                Type = "abonnement_demande_admin",
                EstLue = false,
                DateCreation = DateTime.Now,
                Lien = "/Abonnement/Index"
            });
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Votre demande a ete envoyee. L'abonnement sera active apres validation de l'admin.";
        return RedirectToAction(nameof(MonAbonnement));
    }

    [HttpGet]
    public async Task<IActionResult> MonAbonnement()
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            TempData["Error"] = "Veuillez vous connecter pour acceder a votre abonnement.";
            return RedirectToAction("Login", "Auth");
        }

        await PreparerLayoutMembreAsync("MonAbonnement");

        int membreId = _currentUser.MembreId.Value;
        var abonnement = await GetAbonnementActifAsync(membreId);

        var demandeEnAttente = await GetDemandeEnAttenteAsync(membreId);

        var vm = new MonAbonnementViewModel
        {
            Abonnement = abonnement,
            EstActif = abonnement != null,
            DemandeEnAttente = demandeEnAttente
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Index(string statut = "pending")
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        await _layoutService.PopulateAsync(this, "Abonnements");

        var query = _context.Abonnements
            .Include(a => a.IdMembreNavigation)
            .Include(a => a.IdTypeNavigation)
            .AsQueryable();

        if (statut == "pending")
        {
            query = query.Where(a => a.StatutPaiement == PaiementStatuses.EnAttente);
        }
        else if (statut == "actif")
        {
            query = query.Where(a => a.Statut == AbonnementStatuses.Actif);
        }
        else if (statut == "expired")
        {
            query = query.Where(a => a.Statut != AbonnementStatuses.Actif && a.StatutPaiement == PaiementStatuses.Paye);
        }

        var demandes = await query.OrderByDescending(a => a.IdAbonnment).ToListAsync();

        ViewBag.StatutFiltre = statut;
        ViewBag.CountPending = await _context.Abonnements.CountAsync(a => a.StatutPaiement == PaiementStatuses.EnAttente);
        ViewBag.CountActif = await _context.Abonnements.CountAsync(a => a.Statut == AbonnementStatuses.Actif);
        ViewBag.CountExpired = await _context.Abonnements.CountAsync(a => a.Statut != AbonnementStatuses.Actif && a.StatutPaiement == PaiementStatuses.Paye);

        return View(demandes);
    }

    [HttpGet]
    public async Task<IActionResult> Plans()
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        await _layoutService.PopulateAsync(this, "Abonnements");
        var plans = await _context.TypeAbonnements.OrderBy(t => t.Tarif).ToListAsync();
        return View(plans);
    }

    [HttpGet]
    public async Task<IActionResult> EditPlan(int id)
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        await _layoutService.PopulateAsync(this, "Abonnements");

        var plan = await _context.TypeAbonnements.FindAsync(id);
        if (plan == null)
        {
            TempData["Error"] = "Plan introuvable.";
            return RedirectToAction(nameof(Plans));
        }

        return View(plan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPlan(TypeAbonnement plan)
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        var existing = await _context.TypeAbonnements.FindAsync(plan.IdType);
        if (existing == null)
        {
            TempData["Error"] = "Plan introuvable.";
            return RedirectToAction(nameof(Plans));
        }

        if (string.IsNullOrWhiteSpace(plan.Libelle))
        {
            ModelState.AddModelError("Libelle", "Le libelle est requis.");
        }

        if (plan.Tarif == null || plan.Tarif < 0)
        {
            ModelState.AddModelError("Tarif", "Le tarif doit etre un montant positif.");
        }

        if (!ModelState.IsValid)
        {
            await _layoutService.PopulateAsync(this, "Abonnements");
            return View(plan);
        }

        existing.Libelle = plan.Libelle?.Trim() ?? string.Empty;
        existing.Tarif = plan.Tarif;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Le tarif du plan a ete mis a jour.";
        return RedirectToAction(nameof(Plans));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Valider(int id)
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        var abo = await _context.Abonnements
            .Include(a => a.IdTypeNavigation)
            .FirstOrDefaultAsync(a => a.IdAbonnment == id);

        if (abo == null)
        {
            TempData["Error"] = "Abonnement introuvable.";
            return RedirectToAction(nameof(Index));
        }

        if ((abo.StatutPaiement ?? string.Empty).ToLowerInvariant() != PaiementStatuses.EnAttente)
        {
            TempData["Warning"] = "Cette demande n'est plus en attente.";
            return RedirectToAction(nameof(Index));
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        var abosActifs = await _context.Abonnements
            .Where(a => a.IdMembre == abo.IdMembre
                     && a.IdAbonnment != abo.IdAbonnment
                     && a.Statut == AbonnementStatuses.Actif)
            .ToListAsync();

        foreach (var ancien in abosActifs)
        {
            ancien.Statut = AbonnementStatuses.Inactif;
        }

        abo.DatePaiement = DateTime.Now;
        abo.ReferencePaiement ??= $"PAY-{Guid.NewGuid():N}".Substring(0, 16).ToUpperInvariant();
        abo.StatutPaiement = PaiementStatuses.Paye;
        abo.Statut = AbonnementStatuses.Actif;
        abo.DateDebut = today;
        abo.DateFin = CalculerDateFin(abo.Montant);

        _context.Notifications.Add(new Notification
        {
            IdMembre = abo.IdMembre,
            IdUtilisateur = null,
            Titre = "Abonnement valide",
            Message = $"Votre abonnement \"{abo.IdTypeNavigation?.Libelle}\" a ete valide et active.",
            Type = "abonnement_valide",
            EstLue = false,
            DateCreation = DateTime.Now,
            Lien = "/Abonnement/MonAbonnement"
        });

        await _context.SaveChangesAsync();

        TempData["Success"] = "Paiement valide, abonnement active.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refuser(int id)
    {
        if (!_currentUser.IsAdmin)
        {
            TempData["Error"] = "Acces refuse.";
            return RedirectToAction("Index", "Home");
        }

        var abo = await _context.Abonnements
            .Include(a => a.IdTypeNavigation)
            .FirstOrDefaultAsync(a => a.IdAbonnment == id);

        if (abo == null)
        {
            TempData["Error"] = "Abonnement introuvable.";
            return RedirectToAction(nameof(Index));
        }

        if ((abo.StatutPaiement ?? string.Empty).ToLowerInvariant() != PaiementStatuses.EnAttente)
        {
            TempData["Warning"] = "Cette demande n'est plus en attente.";
            return RedirectToAction(nameof(Index));
        }

        abo.StatutPaiement = PaiementStatuses.Refuse;
        abo.Statut = AbonnementStatuses.Inactif;

        _context.Notifications.Add(new Notification
        {
            IdMembre = abo.IdMembre,
            IdUtilisateur = null,
            Titre = "Demande refusee",
            Message = $"Votre demande pour le plan \"{abo.IdTypeNavigation?.Libelle}\" a ete refusee. Contactez l'administrateur pour plus d'informations.",
            Type = "abonnement_refuse",
            EstLue = false,
            DateCreation = DateTime.Now,
            Lien = "/Abonnement/MonAbonnement"
        });

        await _context.SaveChangesAsync();

        TempData["Success"] = "Demande refusee, membre notifie.";
        return RedirectToAction(nameof(Index));
    }
}
