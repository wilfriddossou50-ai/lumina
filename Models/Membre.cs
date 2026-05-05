using System;
using System.Collections.Generic;

namespace GestionBibliotheque.Models;

public partial class Membre
{
    public int IdMembre { get; set; }

    public string? Nom { get; set; }

    public string? Prenom { get; set; }

    public string? Sexe { get; set; }

    public string? Telephone { get; set; }

    public string? Mail { get; set; }

    public string? Adresse { get; set; }

    public DateOnly? DateInscription { get; set; }

    public string? Statut { get; set; }

    public virtual ICollection<Abonnement> Abonnements { get; set; } = new List<Abonnement>();

    public virtual ICollection<Alerte> Alertes { get; set; } = new List<Alerte>();

    public virtual ICollection<Emprunt> Emprunts { get; set; } = new List<Emprunt>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual ICollection<Utilisateur> Utilisateurs { get; set; } = new List<Utilisateur>();
}
