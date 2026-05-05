using System;
using System.Collections.Generic;

namespace GestionBibliotheque.Models;

public partial class Utilisateur
{
    public int IdUtilisateur { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public string? Role { get; set; }

    public int? IdMembre { get; set; }

    public virtual ICollection<Emprunt> EmpruntIdUserEmpruntNavigations { get; set; } = new List<Emprunt>();

    public virtual ICollection<Emprunt> EmpruntIdUserRetourNavigations { get; set; } = new List<Emprunt>();

    public virtual Membre? IdMembreNavigation { get; set; }
}
