using System;
using System.Collections.Generic;

namespace GestionBibliotheque.Models;

public partial class Categorie
{
    public int IdCategorie { get; set; }

    public string? Libelle { get; set; }

    public virtual ICollection<Livre> Livres { get; set; } = new List<Livre>();
}
