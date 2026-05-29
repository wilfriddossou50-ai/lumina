using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionBibliotheque.Models;

public partial class Notification
{
    [Column("ID_NOTIFICATION")]
    public int IdNotification { get; set; }

    [Column("ID_MEMBRE")]
    public int? IdMembre { get; set; }

    [Column("ID_UTILISATEUR")]
    public int? IdUtilisateur { get; set; }

    [Column("TITRE")]
    public string Titre { get; set; } = null!;

    [Column("MESSAGE")]
    public string? Message { get; set; }

    [Column("DATE_CREATION")]
    public DateTime? DateCreation { get; set; }

    [Column("DATE_LUE")]
    public DateTime? DateLue { get; set; }

    [Column("TYPE")]
    public string? Type { get; set; }

    [Column("EST_LUE")]
    public bool? EstLue { get; set; }

    [Column("LIEN")]
    public string? Lien { get; set; }

    public virtual ICollection<Alerte> Alertes { get; set; } = new List<Alerte>();

    public virtual Membre? IdMembreNavigation { get; set; }
}
