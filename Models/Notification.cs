using System;
using System.Collections.Generic;

namespace GestionBibliotheque.Models;

public partial class Notification
{
    public int IdNotification { get; set; }

    public int IdMembre { get; set; }

    public string Titre { get; set; } = null!;

    public string? Message { get; set; }

    public DateTime? DateCreation { get; set; }

    public DateTime? DateLue { get; set; }

    public string? Type { get; set; }

    public bool? EstLue { get; set; }

    public string? Lien { get; set; }

    public virtual ICollection<Alerte> Alertes { get; set; } = new List<Alerte>();

    public virtual Membre IdMembreNavigation { get; set; } = null!;
}
