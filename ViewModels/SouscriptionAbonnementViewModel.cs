// SouscriptionAbonnementViewModel.cs
using GestionBibliotheque.Models;

namespace GestionBibliotheque.ViewModels;

public class SouscriptionAbonnementViewModel
{
    public List<TypeAbonnement> Plans { get; set; } = new();
    public Abonnement? AbonnementActuel { get; set; }
    public bool EstActif { get; set; }
    public bool EnAttente { get; set; }              // ← IMPORTANT
    public List<PaymentMethodOption> MethodesPaiement { get; set; } = new();
}

public record PaymentMethodOption(string Value, string Label);
