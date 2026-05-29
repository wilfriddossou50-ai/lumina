using System;
using System.Collections.Generic;

namespace GestionBibliotheque.Models;

public partial class Abonnement
{
    public int IdAbonnment { get; set; }

    public int IdMembre { get; set; }

    public int? IdType { get; set; }

    public float? Montant { get; set; }

    public DateOnly? DateDebut { get; set; }

    public DateOnly? DateFin { get; set; }

    public string? Statut { get; set; }

    public string? ModePaiement { get; set; }

    public string? ReferencePaiement { get; set; }

    public DateTime? DatePaiement { get; set; }

    public string? StatutPaiement { get; set; }

    public string? CodeReference { get; set; }

    public virtual Membre IdMembreNavigation { get; set; } = null!;

    public virtual TypeAbonnement? IdTypeNavigation { get; set; }
}
