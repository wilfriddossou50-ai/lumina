using GestionBibliotheque.Constants;
using GestionBibliotheque.Data;
using GestionBibliotheque.Models;
using GestionBibliotheque.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Controllers;

public class CatalogueController : Controller
{
    private readonly BibliothequeDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILayoutService _layoutService;

    public CatalogueController(
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

    private async Task EnsureDefaultCategoriesAsync()
    {
        var categoriesFixed = new[] { "Roman", "Science", "Informatique", "Histoire", "Art" };
        var existingLabels = await _context.Categories
            .Select(c => c.Libelle)
            .ToListAsync();

        bool hasChanges = false;
        foreach (var catName in categoriesFixed)
        {
            if (existingLabels.Contains(catName))
            {
                continue;
            }

            _context.Categories.Add(new Categorie { Libelle = catName });
            hasChanges = true;
        }

        if (hasChanges)
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IActionResult> Index(string searchTerm = "", string category = "")
    {
        if (!_currentUser.IsAuthenticated)
        {
            TempData["Error"] = "Veuillez vous connecter pour acceder au catalogue.";
            return RedirectToAction("Login", "Auth");
        }

        await _layoutService.PopulateAsync(this, "Catalogue");

        ViewData["SearchTerm"] = searchTerm;
        ViewData["SelectedCategory"] = category;

        await EnsureDefaultCategoriesAsync();

        var categoriesFixed = new List<string> { "Roman", "Science", "Informatique", "Histoire", "Art" };
        var query = _context.Livres
            .Include(l => l.IdCategorieNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(l =>
                (l.Titre != null && l.Titre.Contains(searchTerm)) ||
                (l.Auteur != null && l.Auteur.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(l =>
                l.IdCategorieNavigation != null &&
                l.IdCategorieNavigation.Libelle == category);
        }

        var livresList = await query.OrderByDescending(l => l.IdLivre).ToListAsync();

        ViewBag.LivresParCategorie = categoriesFixed.ToDictionary(
            cat => cat,
            cat => livresList.Where(l => l.IdCategorieNavigation?.Libelle == cat).ToList());
        ViewBag.Categories = await _context.Categories.OrderBy(c => c.Libelle).ToListAsync();

        if (_currentUser.IsMembre && _currentUser.MembreId.HasValue)
        {
            int membreId = _currentUser.MembreId.Value;

            var livresEmpruntes = await _context.DetailEmprunts
                .Include(d => d.IdEmpruntNavigation)
                .Where(d => d.IdEmpruntNavigation.IdMembre == membreId
                         && d.IdEmpruntNavigation.Statut == EmpruntStatuses.EnCours
                         && d.DateRetourReel == null)
                .Select(d => d.IdLivre)
                .ToListAsync();

            var livresReserves = await _context.Reservations
                .Where(r => r.IdMembre == membreId
                         && (r.Statut == ReservationStatuses.EnAttente || r.Statut == ReservationStatuses.Disponible))
                .Select(r => r.IdLivre)
                .ToListAsync();

            var livresAlertes = await _context.Alertes
                .Where(a => a.IdMembre == membreId && a.EstTraitee == false)
                .Select(a => a.IdLivre)
                .ToListAsync();

            var abonnement = await GetAbonnementActifAsync(membreId);

            ViewBag.LivresEmpruntes = livresEmpruntes;
            ViewBag.LivresReserves = livresReserves;
            ViewBag.LivresAlertes = livresAlertes;
            ViewBag.AbonnementLibelle = abonnement?.IdTypeNavigation?.Libelle ?? "Gratuit";
            ViewBag.MembreId = membreId;
        }

        return _currentUser.IsAdmin ? View("Index_Admin", livresList) : View("Index_Membre", livresList);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (!_currentUser.IsAuthenticated)
        {
            TempData["Error"] = "Veuillez vous connecter.";
            return RedirectToAction("Login", "Auth");
        }

        if (id == null)
        {
            TempData["Error"] = "ID du livre non specifie.";
            return RedirectToAction(nameof(Index));
        }

        await _layoutService.PopulateAsync(this, "Catalogue");

        var livre = await _context.Livres
            .Include(l => l.IdCategorieNavigation)
            .FirstOrDefaultAsync(l => l.IdLivre == id);

        if (livre == null)
        {
            TempData["Error"] = "Livre non trouve.";
            return RedirectToAction(nameof(Index));
        }

        if (_currentUser.IsMembre && _currentUser.MembreId.HasValue)
        {
            int membreId = _currentUser.MembreId.Value;

            bool dejaEmprunte = await _context.DetailEmprunts
                .Include(d => d.IdEmpruntNavigation)
                .AnyAsync(d => d.IdLivre == id
                            && d.IdEmpruntNavigation.IdMembre == membreId
                            && d.IdEmpruntNavigation.Statut == EmpruntStatuses.EnCours
                            && d.DateRetourReel == null);

            bool dejaReserve = await _context.Reservations
                .AnyAsync(r => r.IdLivre == id
                            && r.IdMembre == membreId
                            && r.Statut == ReservationStatuses.EnAttente);

            bool alerteActive = await _context.Alertes
                .AnyAsync(a => a.IdLivre == id
                            && a.IdMembre == membreId
                            && a.EstTraitee == false);

            int positionFile = 0;
            if (dejaReserve)
            {
                var maResa = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.IdLivre == id
                                           && r.IdMembre == membreId
                                           && r.Statut == ReservationStatuses.EnAttente);

                if (maResa != null)
                {
                    positionFile = await _context.Reservations
                        .CountAsync(r => r.IdLivre == id
                                      && r.Statut == ReservationStatuses.EnAttente
                                      && r.DateReservation < maResa.DateReservation) + 1;
                }
            }

            var abonnement = await GetAbonnementActifAsync(membreId);

            int nbEmpruntsEnCours = await _context.DetailEmprunts
                .Include(d => d.IdEmpruntNavigation)
                .CountAsync(d => d.IdEmpruntNavigation.IdMembre == membreId
                              && d.IdEmpruntNavigation.Statut == EmpruntStatuses.EnCours
                              && d.DateRetourReel == null);

            ViewBag.DejaEmprunte = dejaEmprunte;
            ViewBag.DejaReserve = dejaReserve;
            ViewBag.AlerteActive = alerteActive;
            ViewBag.PositionFile = positionFile;
            ViewBag.AbonnementLibelle = abonnement?.IdTypeNavigation?.Libelle ?? "Gratuit";
            ViewBag.NbEmprunts = nbEmpruntsEnCours;
            ViewBag.MembreId = membreId;
        }

        return View(livre);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Emprunter(int idLivre)
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            return Json(new { success = false, message = "Acces refuse." });
        }

        int membreId = _currentUser.MembreId.Value;

        var livre = await _context.Livres.FindAsync(idLivre);
        if (livre == null)
        {
            return Json(new { success = false, message = "Livre introuvable." });
        }

        if ((livre.QuantiteDisponible ?? 0) <= 0 || livre.Statut == LivreStatuses.Indisponible)
        {
            return Json(new { success = false, message = "Ce livre n'est pas disponible." });
        }

        var abonnement = await GetAbonnementActifAsync(membreId);
        var limites = PlanLimitesConfiguration.GetForTarif(abonnement?.IdTypeNavigation?.Tarif);

        var reservationPrioritaire = await _context.Reservations
            .Where(r => r.IdLivre == idLivre
                     && (r.Statut == ReservationStatuses.Disponible || r.Statut == ReservationStatuses.EnAttente))
            .OrderBy(r => r.Statut == ReservationStatuses.Disponible ? 0 : 1)
            .ThenBy(r => r.DateReservation)
            .ThenBy(r => r.IdReservation)
            .FirstOrDefaultAsync();

        if (reservationPrioritaire != null && reservationPrioritaire.IdMembre != membreId)
        {
            return Json(new { success = false, message = "Ce livre est actuellement reserve pour un autre membre." });
        }

        int nbEnCours = await _context.DetailEmprunts
            .Include(d => d.IdEmpruntNavigation)
            .CountAsync(d => d.IdEmpruntNavigation.IdMembre == membreId
                          && d.IdEmpruntNavigation.Statut == EmpruntStatuses.EnCours
                          && d.DateRetourReel == null);

        if (nbEnCours >= limites.MaxEmpruntsSimultanes)
        {
            return Json(new
            {
                success = false,
                message = $"Vous avez atteint la limite de {limites.MaxEmpruntsSimultanes} emprunts simultanes ({limites.NomPlan})."
            });
        }

        bool dejaEmprunte = await _context.DetailEmprunts
            .Include(d => d.IdEmpruntNavigation)
            .AnyAsync(d => d.IdLivre == idLivre
                        && d.IdEmpruntNavigation.IdMembre == membreId
                        && d.IdEmpruntNavigation.Statut == EmpruntStatuses.EnCours
                        && d.DateRetourReel == null);

        if (dejaEmprunte)
        {
            return Json(new { success = false, message = "Vous avez deja emprunte ce livre." });
        }

        try
        {
            int idUser = _currentUser.UtilisateurId ?? 0;

            var emprunt = new Emprunt
            {
                IdMembre = membreId,
                DateEmprunt = DateOnly.FromDateTime(DateTime.Today),
                IdUserEmprunt = idUser,
                Statut = EmpruntStatuses.EnCours
            };

            _context.Emprunts.Add(emprunt);
            await _context.SaveChangesAsync();

            _context.DetailEmprunts.Add(new DetailEmprunt
            {
                IdEmprunt = emprunt.IdEmprunt,
                IdLivre = idLivre,
                DateRetourPrevu = DateOnly.FromDateTime(DateTime.Today.AddDays(limites.DureeEmpruntJours))
            });

            livre.QuantiteDisponible = (livre.QuantiteDisponible ?? 0) - 1;
            if ((livre.QuantiteDisponible ?? 0) <= 0)
            {
                livre.Statut = LivreStatuses.Indisponible;
            }

            if (reservationPrioritaire != null && reservationPrioritaire.IdMembre == membreId)
            {
                reservationPrioritaire.Statut = ReservationStatuses.Honoree;
            }

            await EnvoyerNotificationAdmin(
                titre: "Nouvel emprunt",
                message: $"{_currentUser.DisplayName} a emprunte \"{livre.Titre}\".",
                type: "emprunt",
                lien: $"/Catalogue/Details/{idLivre}");

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Vous avez emprunte \"{livre.Titre}\". Retour prevu sous {limites.DureeEmpruntJours} jours.";
            return Json(new { success = true, message = TempData["Success"] });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erreur : {ex.Message}" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reserver(int idLivre)
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            return Json(new { success = false, message = "Acces refuse." });
        }

        int membreId = _currentUser.MembreId.Value;

        var livre = await _context.Livres.FindAsync(idLivre);
        if (livre == null)
        {
            return Json(new { success = false, message = "Livre introuvable." });
        }

        if ((livre.QuantiteDisponible ?? 0) > 0 && livre.Statut != LivreStatuses.Indisponible)
        {
            return Json(new { success = false, message = "Ce livre est disponible. Utilisez l'emprunt direct." });
        }

        bool dejaReserve = await _context.Reservations
            .AnyAsync(r => r.IdLivre == idLivre
                        && r.IdMembre == membreId
                        && r.Statut == ReservationStatuses.EnAttente);

        if (dejaReserve)
        {
            return Json(new { success = false, message = "Vous avez deja une reservation active pour ce livre." });
        }

        bool dejaEmprunte = await _context.DetailEmprunts
            .Include(d => d.IdEmpruntNavigation)
            .AnyAsync(d => d.IdLivre == idLivre
                        && d.IdEmpruntNavigation.IdMembre == membreId
                        && d.IdEmpruntNavigation.Statut == EmpruntStatuses.EnCours
                        && d.DateRetourReel == null);

        if (dejaEmprunte)
        {
            return Json(new { success = false, message = "Vous avez deja emprunte ce livre." });
        }

        var abonnement = await GetAbonnementActifAsync(membreId);
        var limites = PlanLimitesConfiguration.GetForTarif(abonnement?.IdTypeNavigation?.Tarif);

        int nbResas = await _context.Reservations
            .CountAsync(r => r.IdMembre == membreId && r.Statut == ReservationStatuses.EnAttente);

        if (nbResas >= limites.MaxReservationsActives)
        {
            return Json(new
            {
                success = false,
                message = $"Limite de {limites.MaxReservationsActives} reservation(s) active(s) atteinte ({limites.NomPlan})."
            });
        }

        try
        {
            int position = await _context.Reservations
                .CountAsync(r => r.IdLivre == idLivre && r.Statut == ReservationStatuses.EnAttente) + 1;

            _context.Reservations.Add(new Reservation
            {
                IdLivre = idLivre,
                IdMembre = membreId,
                DateReservation = DateOnly.FromDateTime(DateTime.Today),
                Statut = ReservationStatuses.EnAttente
            });

            await EnvoyerNotificationAdmin(
                titre: "Nouvelle reservation",
                message: $"{_currentUser.DisplayName} a reserve \"{livre.Titre}\" (position {position} en file).",
                type: "reservation",
                lien: $"/Catalogue/Details/{idLivre}");

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Reservation confirmee pour \"{livre.Titre}\". Vous etes {position}eme en liste.";
            return Json(new { success = true, message = TempData["Success"], position });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erreur : {ex.Message}" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreerAlerte(int idLivre)
    {
        if (!_currentUser.IsMembre || !_currentUser.MembreId.HasValue)
        {
            return Json(new { success = false, message = "Acces refuse." });
        }

        int membreId = _currentUser.MembreId.Value;

        var livre = await _context.Livres.FindAsync(idLivre);
        if (livre == null)
        {
            return Json(new { success = false, message = "Livre introuvable." });
        }

        if ((livre.QuantiteDisponible ?? 0) > 0 && livre.Statut != LivreStatuses.Indisponible)
        {
            return Json(new { success = false, message = "Ce livre est deja disponible. Vous pouvez l'emprunter directement." });
        }

        bool alerteExiste = await _context.Alertes
            .AnyAsync(a => a.IdLivre == idLivre
                        && a.IdMembre == membreId
                        && a.EstTraitee == false);

        if (alerteExiste)
        {
            return Json(new { success = false, message = "Vous avez deja une alerte active pour ce livre." });
        }

        var abonnement = await GetAbonnementActifAsync(membreId);
        var limites = PlanLimitesConfiguration.GetForTarif(abonnement?.IdTypeNavigation?.Tarif);

        int nbAlertes = await _context.Alertes
            .CountAsync(a => a.IdMembre == membreId && a.EstTraitee == false);

        if (nbAlertes >= limites.MaxAlertesActives)
        {
            return Json(new
            {
                success = false,
                message = $"Limite de {limites.MaxAlertesActives} alerte(s) active(s) atteinte ({limites.NomPlan})."
            });
        }

        try
        {
            _context.Alertes.Add(new Alerte
            {
                IdMembre = membreId,
                IdLivre = idLivre,
                DateDemande = DateOnly.FromDateTime(DateTime.Today),
                MoyenNotification = "systeme",
                EstTraitee = false
            });

            await EnvoyerNotificationAdmin(
                titre: "Demande d'alerte disponibilite",
                message: $"{_currentUser.DisplayName} souhaite etre notifie quand \"{livre.Titre}\" sera disponible.",
                type: "alerte",
                lien: $"/Catalogue/Details/{idLivre}");

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Alerte creee. Vous serez notifie des que \"{livre.Titre}\" sera disponible.";
            return Json(new { success = true, message = TempData["Success"] });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Erreur : {ex.Message}" });
        }
    }

    public async Task<IActionResult> Create()
    {
        if (!_currentUser.IsAdmin)
        {
            return RedirectToAction("Index", "Home");
        }

        await _layoutService.PopulateAsync(this, "Catalogue");
        ViewBag.Categories = await _context.Categories.OrderBy(c => c.Libelle).ToListAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Livre livre)
    {
        if (!_currentUser.IsAdmin)
        {
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(livre.Titre))
        {
            ModelState.AddModelError("Titre", "Le titre est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(livre.Description))
        {
            ModelState.AddModelError("Description", "La description est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(livre.Langue))
        {
            ModelState.AddModelError("Langue", "La langue est obligatoire.");
        }

        if (livre.IdCategorie == null || livre.IdCategorie == 0)
        {
            ModelState.AddModelError("IdCategorie", "Veuillez selectionner une categorie.");
        }

        if (!ModelState.IsValid)
        {
            await _layoutService.PopulateAsync(this, "Catalogue");
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Libelle).ToListAsync();
            TempData["Error"] = "Veuillez remplir tous les champs obligatoires.";
            return View(livre);
        }

        livre.QuantiteTotal = livre.QuantiteTotal is null or <= 0 ? 1 : livre.QuantiteTotal;
        livre.QuantiteDisponible ??= livre.QuantiteTotal;
        livre.Statut = string.IsNullOrWhiteSpace(livre.Statut) ? LivreStatuses.Disponible : livre.Statut;

        _context.Livres.Add(livre);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Le livre \"{livre.Titre}\" a ete ajoute.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (!_currentUser.IsAdmin)
        {
            return RedirectToAction("Index", "Home");
        }

        if (id == null)
        {
            return RedirectToAction(nameof(Index));
        }

        var livre = await _context.Livres.FindAsync(id);
        if (livre == null)
        {
            TempData["Error"] = "Livre non trouve.";
            return RedirectToAction(nameof(Index));
        }

        await _layoutService.PopulateAsync(this, "Catalogue");
        ViewBag.Categories = await _context.Categories.OrderBy(c => c.Libelle).ToListAsync();
        return View(livre);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Livre livre)
    {
        if (!_currentUser.IsAdmin)
        {
            return RedirectToAction(nameof(Index));
        }

        if (id != livre.IdLivre)
        {
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(livre.Titre))
        {
            ModelState.AddModelError("Titre", "Le titre est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(livre.Description))
        {
            ModelState.AddModelError("Description", "La description est obligatoire.");
        }

        if (!ModelState.IsValid)
        {
            await _layoutService.PopulateAsync(this, "Catalogue");
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Libelle).ToListAsync();
            TempData["Error"] = "Veuillez corriger les erreurs.";
            return View(livre);
        }

        try
        {
            _context.Livres.Update(livre);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Le livre \"{livre.Titre}\" a ete mis a jour.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!LivreExists(livre.IdLivre))
            {
                TempData["Error"] = "Livre supprime.";
                return RedirectToAction(nameof(Index));
            }

            throw;
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromBody] int? id)
    {
        if (!_currentUser.IsAdmin)
        {
            return Json(new { success = false, message = "Acces refuse." });
        }

        if (id == null)
        {
            return Json(new { success = false, message = "ID invalide." });
        }

        var livre = await _context.Livres
            .Include(l => l.DetailEmprunts)
            .Include(l => l.Reservations)
            .Include(l => l.Alertes)
            .FirstOrDefaultAsync(l => l.IdLivre == id);

        if (livre == null)
        {
            return Json(new { success = false, message = "Livre non trouve." });
        }

        if (livre.DetailEmprunts.Count > 0 || livre.Reservations.Count > 0 || livre.Alertes.Count > 0)
        {
            return Json(new
            {
                success = false,
                message = "Ce livre ne peut pas etre supprime car il possede deja un historique d'emprunts, de reservations ou d'alertes."
            });
        }

        _context.Livres.Remove(livre);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = $"Le livre \"{livre.Titre}\" a ete supprime." });
    }

    private async Task EnvoyerNotificationAdmin(string titre, string message, string type, string lien)
    {
        var adminIds = await _context.Utilisateurs
            .Where(u => u.Role == ApplicationRoles.Admin)
            .Select(u => u.IdUtilisateur)
            .ToListAsync();

        foreach (var adminId in adminIds)
        {
            _context.Notifications.Add(new Notification
            {
                IdUtilisateur = adminId,
                Titre = titre,
                Message = message,
                Type = type,
                Lien = lien,
                DateCreation = DateTime.Now,
                EstLue = false
            });
        }
    }

    private bool LivreExists(int id) => _context.Livres.Any(e => e.IdLivre == id);
}
