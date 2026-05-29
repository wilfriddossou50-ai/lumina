using BCrypt.Net;
using GestionBibliotheque.Constants;
using GestionBibliotheque.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionBibliotheque.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(BibliothequeDbContext context)
    {
        if (await context.Utilisateurs.AnyAsync())
        {
            return;
        }

        var categories = new List<Categorie>
        {
            new() { IdCategorie = 1, Libelle = "Roman" },
            new() { IdCategorie = 2, Libelle = "Science" },
            new() { IdCategorie = 3, Libelle = "Informatique" },
            new() { IdCategorie = 4, Libelle = "Histoire" },
            new() { IdCategorie = 5, Libelle = "Art" }
        };

        var types = new List<TypeAbonnement>
        {
            new() { IdType = 1, Libelle = "Gratuit", Tarif = 0 },
            new() { IdType = 2, Libelle = "Standard", Tarif = 15 },
            new() { IdType = 3, Libelle = "Premium", Tarif = 30 }
        };

        var membres = new List<Membre>
        {
            new()
            {
                IdMembre = 1,
                Nom = "Dupont",
                Prenom = "Jean",
                Sexe = "Masculin",
                Telephone = "+33 6 12 34 56 78",
                Mail = "jean.dupont@example.com",
                Adresse = "12 rue de la Bibliothèque, Paris",
                DateInscription = DateOnly.FromDateTime(DateTime.Today.AddMonths(-4)),
                Statut = "actif"
            },
            new()
            {
                IdMembre = 2,
                Nom = "Kone",
                Prenom = "Amina",
                Sexe = "Féminin",
                Telephone = "+33 6 87 65 43 21",
                Mail = "amina.kone@example.com",
                Adresse = "45 avenue des Arts, Lyon",
                DateInscription = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
                Statut = "actif"
            }
        };

        var livres = new List<Livre>
        {
            new()
            {
                IdLivre = 1,
                Isbn = "9782070368226",
                Titre = "Le Petit Prince",
                Description = "Un classique de la littérature française.",
                Langue = "Français",
                Auteur = "Antoine de Saint-Exupéry",
                Editeur = "Gallimard",
                AnneePublication = new DateOnly(1943, 4, 6),
                IdCategorie = 1,
                QuantiteTotal = 5,
                QuantiteDisponible = 2,
                Emplacement = "R1-A2",
                Statut = "disponible"
            },
            new()
            {
                IdLivre = 2,
                Isbn = "9782266165839",
                Titre = "La Machine à explorer le temps",
                Description = "Un récit de science-fiction sur le voyage dans le temps.",
                Langue = "Français",
                Auteur = "H. G. Wells",
                Editeur = "Le Livre de Poche",
                AnneePublication = new DateOnly(1895, 5, 7),
                IdCategorie = 2,
                QuantiteTotal = 3,
                QuantiteDisponible = 0,
                Emplacement = "R2-B1",
                Statut = "indisponible"
            },
            new()
            {
                IdLivre = 3,
                Isbn = "9782212560513",
                Titre = "C# pour tous",
                Description = "Guide pratique pour apprendre le langage C#.",
                Langue = "Français",
                Auteur = "Aurélien Bodin",
                Editeur = "Dunod",
                AnneePublication = new DateOnly(2023, 1, 15),
                IdCategorie = 3,
                QuantiteTotal = 4,
                QuantiteDisponible = 1,
                Emplacement = "R3-C4",
                Statut = "disponible"
            },
            new()
            {
                IdLivre = 4,
                Isbn = "9782060000000",
                Titre = "Histoire de France",
                Description = "Une synthèse des grands événements de l'histoire française.",
                Langue = "Français",
                Auteur = "Sophie Martin",
                Editeur = "Seuil",
                AnneePublication = new DateOnly(2018, 9, 1),
                IdCategorie = 4,
                QuantiteTotal = 2,
                QuantiteDisponible = 0,
                Emplacement = "R4-D3",
                Statut = "disponible"
            },
            new()
            {
                IdLivre = 5,
                Isbn = "9782212560520",
                Titre = "L'Art du geste",
                Description = "Essai sur la créativité et le mouvement artistique.",
                Langue = "Français",
                Auteur = "Marie Leduc",
                Editeur = "Flammarion",
                AnneePublication = new DateOnly(2021, 4, 12),
                IdCategorie = 5,
                QuantiteTotal = 1,
                QuantiteDisponible = 0,
                Emplacement = "R5-E2",
                Statut = "indisponible"
            },
            new()
            {
                IdLivre = 6,
                Isbn = "9782345678901",
                Titre = "Mathématiques discrètes",
                Description = "Cours et exercices sur les fondamentaux mathématiques.",
                Langue = "Français",
                Auteur = "Pierre Laurent",
                Editeur = "Ellipses",
                AnneePublication = new DateOnly(2020, 2, 20),
                IdCategorie = 2,
                QuantiteTotal = 3,
                QuantiteDisponible = 2,
                Emplacement = "R2-B2",
                Statut = "disponible"
            },
            new()
            {
                IdLivre = 7,
                Isbn = "9782376870001",
                Titre = "Design UX moderne",
                Description = "Principes et bonnes pratiques du design d'expérience utilisateur.",
                Langue = "Français",
                Auteur = "Claire Dubois",
                Editeur = "Eyrolles",
                AnneePublication = new DateOnly(2022, 7, 9),
                IdCategorie = 5,
                QuantiteTotal = 5,
                QuantiteDisponible = 5,
                Emplacement = "R5-E5",
                Statut = "disponible"
            },
            new()
            {
                IdLivre = 8,
                Isbn = "9781234567806",
                Titre = "Roman noir",
                Description = "Polar haletant situé dans une grande ville.",
                Langue = "Français",
                Auteur = "Marc Lemoine",
                Editeur = "Pocket",
                AnneePublication = new DateOnly(2019, 10, 2),
                IdCategorie = 1,
                QuantiteTotal = 6,
                QuantiteDisponible = 6,
                Emplacement = "R1-A5",
                Statut = "disponible"
            },
            new()
            {
                IdLivre = 9,
                Isbn = "9789876543210",
                Titre = "Programmation web",
                Description = "Techniques modernes pour créer des applications web.",
                Langue = "Français",
                Auteur = "Nadia Petit",
                Editeur = "O'Reilly",
                AnneePublication = new DateOnly(2024, 3, 18),
                IdCategorie = 3,
                QuantiteTotal = 8,
                QuantiteDisponible = 8,
                Emplacement = "R3-C1",
                Statut = "disponible"
            },
            new()
            {
                IdLivre = 10,
                Isbn = "9782222222222",
                Titre = "Géopolitique contemporaine",
                Description = "Analyse des grands enjeux mondiaux actuels.",
                Langue = "Français",
                Auteur = "Luc Perrin",
                Editeur = "CNRS Editions",
                AnneePublication = new DateOnly(2021, 11, 28),
                IdCategorie = 4,
                QuantiteTotal = 3,
                QuantiteDisponible = 3,
                Emplacement = "R4-D1",
                Statut = "disponible"
            }
        };

        var utilisateurs = new List<Utilisateur>
        {
            new()
            {
                IdUtilisateur = 1,
                Login = "admin",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@2026"),
                Role = ApplicationRoles.Admin
            },
            new()
            {
                IdUtilisateur = 2,
                Login = "jean",
                Password = BCrypt.Net.BCrypt.HashPassword("Membre@2026"),
                Role = ApplicationRoles.Membre,
                IdMembre = 1
            },
            new()
            {
                IdUtilisateur = 3,
                Login = "amina",
                Password = BCrypt.Net.BCrypt.HashPassword("Membre@2026"),
                Role = ApplicationRoles.Membre,
                IdMembre = 2
            }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.TypeAbonnements.AddRangeAsync(types);
        await context.Membres.AddRangeAsync(membres);
        await context.Livres.AddRangeAsync(livres);
        await context.Utilisateurs.AddRangeAsync(utilisateurs);
        await context.SaveChangesAsync();

        var abonnements = new List<Abonnement>
        {
            new()
            {
                IdAbonnment = 1,
                IdMembre = 1,
                IdType = 3,
                Montant = 30,
                DateDebut = DateOnly.FromDateTime(DateTime.Today.AddDays(-30)),
                DateFin = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
                Statut = "actif",
                ModePaiement = "Carte",
                ReferencePaiement = "PAY-2026-001",
                DatePaiement = DateTime.Today.AddDays(-30),
                StatutPaiement = "paye",
                CodeReference = "PREM-001"
            },
            new()
            {
                IdAbonnment = 2,
                IdMembre = 2,
                IdType = 2,
                Montant = 15,
                DateDebut = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
                DateFin = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
                Statut = "actif",
                ModePaiement = "Carte",
                ReferencePaiement = "PAY-2026-002",
                DatePaiement = DateTime.Today.AddDays(-10),
                StatutPaiement = "paye",
                CodeReference = "STD-002"
            }
        };

        await context.Abonnements.AddRangeAsync(abonnements);
        await context.SaveChangesAsync();

        var emprunts = new List<Emprunt>
        {
            new()
            {
                IdEmprunt = 1,
                IdMembre = 1,
                DateEmprunt = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
                IdUserEmprunt = 1,
                Statut = "en_cours"
            },
            new()
            {
                IdEmprunt = 2,
                IdMembre = 1,
                DateEmprunt = DateOnly.FromDateTime(DateTime.Today.AddDays(-25)),
                IdUserEmprunt = 1,
                IdUserRetour = 1,
                Statut = "en_retard"
            }
        };

        await context.Emprunts.AddRangeAsync(emprunts);
        await context.SaveChangesAsync();

        var detailEmprunts = new List<DetailEmprunt>
        {
            new()
            {
                IdEmprunt = 1,
                IdLivre = 1,
                DateRetourPrevu = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                MontantPenalite = 0
            },
            new()
            {
                IdEmprunt = 2,
                IdLivre = 2,
                DateRetourPrevu = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)),
                MontantPenalite = 12.50m,
                NbJourRetard = 5
            }
        };

        await context.DetailEmprunts.AddRangeAsync(detailEmprunts);
        await context.SaveChangesAsync();

        var reservations = new List<Reservation>
        {
            new()
            {
                IdReservation = 1,
                IdLivre = 3,
                IdMembre = 2,
                DateReservation = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
                Statut = "en_attente"
            },
            new()
            {
                IdReservation = 2,
                IdLivre = 4,
                IdMembre = 1,
                DateReservation = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)),
                Statut = "disponible"
            }
        };

        await context.Reservations.AddRangeAsync(reservations);

        var notifications = new List<Notification>
        {
            new()
            {
                IdNotification = 1,
                IdMembre = 1,
                Titre = "Retour proche",
                Message = "Votre emprunt approche de sa date de retour.",
                DateCreation = DateTime.Now.AddDays(-1),
                EstLue = false,
                Type = "info",
                Lien = "/Emprunt/MesEmprunts"
            },
            new()
            {
                IdNotification = 2,
                IdMembre = 1,
                Titre = "Alerte créée",
                Message = "Une alerte a été créée pour un livre indisponible.",
                DateCreation = DateTime.Now.AddHours(-4),
                EstLue = false,
                Type = "alerte"
            },
            new()
            {
                IdNotification = 3,
                IdMembre = 2,
                Titre = "Réservation en cours",
                Message = "Votre réservation a bien été prise en compte.",
                DateCreation = DateTime.Now.AddDays(-1),
                EstLue = true,
                Type = "info"
            }
        };

        await context.Notifications.AddRangeAsync(notifications);
        await context.SaveChangesAsync();

        var alertes = new List<Alerte>
        {
            new()
            {
                IdAlerte = 1,
                IdMembre = 1,
                IdLivre = 5,
                DateDemande = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)),
                EstTraitee = false,
                MoyenNotification = "email"
            }
        };

        await context.Alertes.AddRangeAsync(alertes);
        await context.SaveChangesAsync();
    }
}
