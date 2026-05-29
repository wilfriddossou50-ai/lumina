using System;

namespace GestionBibliotheque.ViewModels
{
    public class ActiviteRecenteViewModel
    {
        public string? NomMembre { get; set; }

        public string? TitreLivre { get; set; }

        public DateOnly? DateEmprunt { get; set; }

        public string? Statut { get; set; }
    }
}