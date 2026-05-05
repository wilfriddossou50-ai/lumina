using System;
using System.Collections.Generic;

namespace GestionBibliotheque.Models;

public partial class DetailEmprunt
{
    public int IdEmprunt { get; set; }

    public int IdLivre { get; set; }

    public DateOnly? DateRetourPrevu { get; set; }

    public DateOnly? DateRetourReel { get; set; }

    public int? NbJourRetard { get; set; }

    public decimal? MontantPenalite { get; set; }

    public virtual Emprunt IdEmpruntNavigation { get; set; } = null!;

    public virtual Livre IdLivreNavigation { get; set; } = null!;
}
