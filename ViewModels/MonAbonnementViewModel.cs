// MonAbonnementViewModel.cs
using GestionBibliotheque.Models;

namespace GestionBibliotheque.ViewModels;

public class MonAbonnementViewModel
{
    public Abonnement? Abonnement { get; set; }
    public bool EstActif { get; set; }
    public Abonnement? DemandeEnAttente { get; set; } // ← IMPORTANT
}
