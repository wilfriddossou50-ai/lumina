using System;
using System.Collections.Generic;

namespace GestionBibliotheque.Models;

public partial class Emprunt
{
    public int IdEmprunt { get; set; }

    public int IdMembre { get; set; }

    public DateOnly? DateEmprunt { get; set; }

    public int IdUserEmprunt { get; set; }

    public int? IdUserRetour { get; set; }

    public string? Statut { get; set; }

    public virtual ICollection<DetailEmprunt> DetailEmprunts { get; set; } = new List<DetailEmprunt>();

    public virtual Membre IdMembreNavigation { get; set; } = null!;

    public virtual Utilisateur IdUserEmpruntNavigation { get; set; } = null!;

    public virtual Utilisateur? IdUserRetourNavigation { get; set; }
}
