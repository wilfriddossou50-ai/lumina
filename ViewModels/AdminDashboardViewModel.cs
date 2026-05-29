using System.Collections.Generic;

namespace GestionBibliotheque.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalLivres { get; set; }
    public int TotalExemplaires { get; set; }
    public int TotalMembres { get; set; }
    public int TotalEmpruntsEnCours { get; set; }
    public int TotalRetards { get; set; }
    public int LivresIndisponibles { get; set; }
    public int TotalReservations { get; set; }
    public int NbNotificationsNonLues { get; set; }
    public int LivresDisponibles { get; set; }
    public int NouveauxMembresCeMois { get; set; }
    public int EmpruntsCetteSemaine { get; set; }
    public int RetoursPrevusCetteSemaine { get; set; }
    public int TauxDisponibilite { get; set; }
    public int TauxOccupation { get; set; }
    public int TauxRetard { get; set; }
    public int TauxReservation { get; set; }
    public string MessagePilotage { get; set; } = string.Empty;
    public string NiveauService { get; set; } = string.Empty;
    public List<ActiviteRecenteViewModel> ActivitesRecentes { get; set; } = new();
    public List<DashboardTrendPointViewModel> EmpruntsHebdomadaires { get; set; } = new();
    public List<DashboardCategoryStatViewModel> CategoriesPopulaires { get; set; } = new();
}

public class DashboardTrendPointViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class DashboardCategoryStatViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public int Percentage { get; set; }
}
