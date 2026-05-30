# Rapport — Fonctionnalités et flux (GestionBibliotheque)

Résumé : Ce document regroupe les fonctionnalités principales du projet, organisées par modules, et décrit les flux utilisateurs et techniques pour les scénarios critiques (authentification, consultation catalogue, emprunt, réservation, notifications, administration). Il sert de base pour le rapport de projet et la documentation opérationnelle.

## Table des matières
- Modules
  - Authentification & Utilisateurs
  - Catalogue & Livres
  - Emprunt & Retour
  - Réservation
  - Abonnements
  - Notifications & Alertes
  - Administration
  - Services & Infrastructure
  - Data & Migrations
- Flux principaux (séquences)
- Points d'intégration et déploiement
- Vérifications et tests recommandés

---

## Modules

### 1) Authentification & Utilisateurs
Fichiers clés : `Controllers/AuthController.cs`, `Models/Utilisateur.cs`, `Models/Membre.cs`, `Services/CurrentUserService.cs`.

Fonctionnalités:
- Inscription (`AuthController.Inscription`) — création de `Utilisateur` et éventuellement `Membre`.
- Connexion (`AuthController.Login`) — vérification du mot de passe (BCrypt), création de session.
- Gestion du profil (`ProfilController`) — affichage et modification des données utilisateur/membre.
- Gestion des rôles : constantes dans `Constants/ApplicationRoles.cs`.

Flux utilisateur (auth):
1. Utilisateur soumet formulaire d'inscription ou login.
2. `AuthController` valide, utilise `BCrypt.Net` pour hasher/vérifier mot de passe.
3. En cas de succès, session utilisateur sauvegardée et redirection.

Test rapide : créer utilisateurs de test (admin/Admin@2026, jean/Membre@2026, amina/Membre@2026).

---

### 2) Catalogue & Livres
Fichiers clés : `Controllers/CatalogueController.cs`, `Models/Livre.cs`, `Views/Catalogue/`.

Fonctionnalités:
- Liste des livres (pagination, filtrage par catégorie).
- Détail d'un livre (infos, disponibilité).
- Composant `_LivreCard.cshtml` pour affichage uniforme.

Flux :
- Utilisateur récupère la liste via `CatalogueController.Index` → ViewModel remplit livres + catégories.

---

### 3) Emprunt & Retour
Fichiers clés : `Controllers/EmpruntController.cs`, `Models/Emprunt.cs`, `Models/DetailEmprunt.cs`, `Services/` (logique métier dans contrôleurs ou services).

Fonctionnalités:
- Créer un emprunt (vérifier disponibilité, créer `Emprunt` + `DetailEmprunt`).
- Mettre à jour le statut de retour et calculer pénalités si en retard.
- Visualiser emprunts de l'utilisateur (MemberDashboard).

Flux emprunt :
1. Utilisateur clique "Emprunter" sur un livre disponible.
2. `EmpruntController` vérifie règles (abonnement, disponibilités).
3. Création de `Emprunt` et `DetailEmprunt`, mise à jour `Livre.Quantite`.
4. Notification éventuelle (création d'une `Notification` pour le membre).

---

### 4) Réservation
Fichiers clés : `Controllers/ReservationController.cs`, `Models/Reservation.cs`.

Fonctionnalités:
- Réserver un livre indisponible ou prochainement disponible.
- Gérer file d'attente de réservations (statuts: en_attente, disponible, annulée).
- Notification quand livre disponible.

Flux réservation :
1. Création de `Reservation` avec `IdLivre` et `IdMembre`.
2. Lorsqu'un livre est rendu et disponible, système (manuellement ou via job) marque la réservation comme `disponible` et notifie.

---

### 5) Abonnements
Fichiers clés : `Controllers/AbonnementController.cs`, `Models/Abonnement.cs`, `Models/TypeAbonnement.cs`, `Views/Abonnement/`.

Fonctionnalités:
- Visualiser plans (`Plans.cshtml`), souscrire ou modifier abonnement.
- Gérer statut `Actif`/`Inactif`, montant payé.

Flux :
- Utilisateur choisit un `TypeAbonnement`, `Abonnement` créé et lié au `Membre`.

---

### 6) Notifications & Alertes
Fichiers clés : `Controllers/NotificationController.cs`, `Controllers/AlerteController.cs`, `Models/Notification.cs`, `Models/Alerte.cs`.

Fonctionnalités:
- Notifications internes (read/unread) pour événements (réservations, retards, alertes).
- Alertes administratives liées à livres ou membres.

Flux :
- Actions métier (retard, disponibilité) génèrent en DB une `Notification` ou `Alerte`.
- Vue `MemberDashboard` / `MesAlertes` présente ces éléments.

---

### 7) Administration
Fichiers clés : `Controllers/AdminDashboardController.cs`, vues `Views/AdminDashboard/`.

Fonctionnalités:
- Dashboard admin (statistiques, gestion livres/membres/alertes).
- Boutons d'administration (styles dans `wwwroot/css/admin.css` et `admin-layout.css`).

Flux admin :
- Admin authentifié accède au dashboard et effectue opérations CRUD sur entités.

---

### 8) Services & Infrastructure
Fichiers clés : `Services/LayoutService.cs`, `Services/CurrentUserService.cs`, `Program.cs`.

Fonctionnalités:
- Services pour récupérer utilisateur courant, layout partiel.
- Gestion de session (`AddSession`) et DI.

Flow technique au démarrage (`Program.cs`):
1. Configuration des services, DbContext (Pomelo MySQL) via `builder.Configuration.GetConnectionString("DefaultConnection")`.
2. Au build, exécution de `context.Database.MigrateAsync();` puis `DbInitializer.InitializeAsync(context);` pour appliquer migrations et seed.

---

### 9) Data & Migrations
Fichiers clés : `Data/BibliothequeDbContext.cs`, `Data/DbInitializer.cs`, dossier `Migrations/`.

Fonctionnalités:
- Schéma EF Core pour toutes les entités (Livre, Membre, Emprunt, etc.).
- Seed complet (ex. 10 livres, 3 utilisateurs, abonnements, emprunts, réservations, notifications).

Flux migration/seed :
- `Program.cs` exécute `MigrateAsync()` au startup puis le seed si nécessaire.

---

## Flux principaux détaillés (séquences)

1) Connexion + navigation catalogue
- Utilisateur → `AuthController.Login` → session
- Accès `CatalogueController.Index` → affichage livres et catégories

2) Emprunt d'un livre
- `CatalogueController` (ou détail) → action `Emprunter`
- `EmpruntController` vérifie disponibilité → crée `Emprunt` et `DetailEmprunt`
- Décrémente `Livre.Quantite`; si réserve en attente, notifie le prochain membre

3) Retour et notification
- Utilisateur rend livre → `EmpruntController` met à jour `DetailEmprunt.DateRetour` et calcule pénalité
- Si réservations existantes, change statut et notifie

4) Réservation puis disponibilité
- `ReservationController.Create` → en_attente
- À disponibilité (retour traité) → `Reservation` devient `disponible` → `Notification` envoyée

---

## Points d'intégration & déploiement

- Base de données : MySQL via Pomelo (`Pomelo.EntityFrameworkCore.MySql`) — chaîne de connexion dans `appsettings.json` (ex: Railway).
- Déploiement : GitHub repo `wilfriddossou50-ai/lumina` connecté à Railway; build `dotnet publish` et exécution du binaire.
- CI: Railway déclenche déploiement sur push `main`.

---

## Vérifications et tests recommandés

- Tests fonctionnels manuels :
  - Inscription / login (admin et membres)
  - Parcours catalogue → emprunt → retour → vérif. quantités
  - Réservation workflow (création + notification)
  - Souscription abonnement (changer limites si applicable)
  - Vérifier notifications et alertes

- Vérifications infra :
  - Confirmer `ConnectionStrings__DefaultConnection` sur Railway
  - Logs d'app pour erreurs de migration/seed

- Cas limites :
  - Emprunter quand pas d'abonnement ou quota atteint
  - Réserver alors que livre devient indisponible
  - Gestion des retards (pénalités)

---

## Annexes rapides
- Controllers principaux :
  - `Controllers/AuthController.cs`
  - `Controllers/CatalogueController.cs`
  - `Controllers/EmpruntController.cs`
  - `Controllers/ReservationController.cs`
  - `Controllers/AbonnementController.cs`
  - `Controllers/NotificationController.cs`
  - `Controllers/AlerteController.cs`
  - `Controllers/AdminDashboardController.cs`
  - `Controllers/MemberDashboardController.cs`
  - `Controllers/MembreController.cs`
  - `Controllers/ProfilController.cs`

- Modèles : `Models/*.cs` incluent `Livre`, `Membre`, `Emprunt`, `DetailEmprunt`, `Reservation`, `Notification`, `Alerte`, `TypeAbonnement`, `Abonnement`, `Utilisateur`.

---

## Prochaines actions proposées
- Si tu veux, j'ajoute des diagrammes de séquence (Mermaid) pour les flux clés.
- Générer une version PDF du `REPORT.md` prête pour ton rendu.
- Produire un guide de test pas-à-pas (checklist) pour la validation finale.

---

_Fin du rapport synthétique. (Fichier créé : `REPORT.md`)"