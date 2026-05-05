using System;
using System.Collections.Generic;

namespace GestionBibliotheque.Models;

public partial class Reservation
{
    public int IdReservation { get; set; }

    public int? IdLivre { get; set; }

    public int IdMembre { get; set; }

    public DateOnly? DateReservation { get; set; }

    public string? Statut { get; set; }

    public virtual Livre? IdLivreNavigation { get; set; }

    public virtual Membre IdMembreNavigation { get; set; } = null!;
}
