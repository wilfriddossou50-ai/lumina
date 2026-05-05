using System;
using System.Collections.Generic;

namespace GestionBibliotheque.Models;

public partial class Livre
{
    public int IdLivre { get; set; }

    public string? Isbn { get; set; }

    public string? Titre { get; set; }

    public string Description { get; set; } = null!;

    public string Langue { get; set; } = null!;

    public string? Auteur { get; set; }

    public string? Editeur { get; set; }

    public DateOnly? AnneePublication { get; set; }

    public int? IdCategorie { get; set; }

    public decimal? QuantiteTotal { get; set; }

    public decimal? QuantiteDisponible { get; set; }

    public string? Emplacement { get; set; }

    public string? Statut { get; set; }

    public virtual ICollection<Alerte> Alertes { get; set; } = new List<Alerte>();

    public virtual ICollection<DetailEmprunt> DetailEmprunts { get; set; } = new List<DetailEmprunt>();

    public virtual Categorie? IdCategorieNavigation { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
