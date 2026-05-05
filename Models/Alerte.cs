using System;
using System.Collections.Generic;

namespace GestionBibliotheque.Models;

public partial class Alerte
{
    public int IdAlerte { get; set; }

    public int IdMembre { get; set; }

    public int IdLivre { get; set; }

    public DateOnly? DateDemande { get; set; }

    public DateOnly? DateNotification { get; set; }

    public string? MoyenNotification { get; set; }

    public bool? EstTraitee { get; set; }

    public int? IdNotification { get; set; }

    public virtual Livre IdLivreNavigation { get; set; } = null!;

    public virtual Membre IdMembreNavigation { get; set; } = null!;

    public virtual Notification? IdNotificationNavigation { get; set; }
}
