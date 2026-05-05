using System;
using System.Collections.Generic;

namespace GestionBibliotheque.Models;

public partial class TypeAbonnement
{
    public int IdType { get; set; }

    public string? Libelle { get; set; }

    public float? Tarif { get; set; }

    public virtual ICollection<Abonnement> Abonnements { get; set; } = new List<Abonnement>();
}
