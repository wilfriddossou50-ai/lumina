namespace GestionBibliotheque.Services;

/// <summary>
/// Limites métier par plan d’abonnement, identifiées par le <b>tarif FCFA</b> stocké sur <see cref="Models.TypeAbonnement.Tarif"/>.
/// Hors bandes connues → règles du plan gratuit (sécurité).
/// </summary>
public static class PlanLimitesConfiguration
{
    /// <summary>Tarifs officiels (FCFA) — à garder alignés avec la base <c>type_abonnement.TARIF</c>.</summary>
    public const float TarifGratuit = 0f;
    public const float TarifEssentiel30j = 700f;
    public const float TarifStandard30j = 1000f;
    public const float TarifAnnuel = 5000f;

    private const float ToléranceTarif = 0.5f;

    /// <summary>
    /// Grille proposée :
    /// <list type="table">
    /// <item><term>0 FCFA</term><description>Gratuit / illimité en durée : 2 emprunts, 1 résa, 2 alertes, 14 j d’emprunt.</description></item>
    /// <item><term>700 FCFA (30 j)</term><description>Essentiel : 3 emprunts, 2 résas, 3 alertes, 21 j.</description></item>
    /// <item><term>1000 FCFA (30 j)</term><description>Standard : 5 emprunts, 4 résas, 5 alertes, 21 j.</description></item>
    /// <item><term>5000 FCFA (1 an)</term><description>Premium annuel : 10 emprunts, 8 résas, 10 alertes, 30 j.</description></item>
    /// </list>
    /// </summary>
    public static PlanLimites GetForTarif(float? tarifMontant)
    {
        float t = tarifMontant ?? TarifGratuit;

        if (t <= ToléranceTarif)
            return PlanLimites.Gratuit;

        if (Math.Abs(t - TarifEssentiel30j) < ToléranceTarif)
            return PlanLimites.Essentiel;

        if (Math.Abs(t - TarifStandard30j) < ToléranceTarif)
            return PlanLimites.Standard;

        if (Math.Abs(t - TarifAnnuel) < ToléranceTarif)
            return PlanLimites.Annuel;

        return PlanLimites.Gratuit;
    }
}

/// <param name="NomPlan">Libellé affichable (résumé de la grille).</param>
/// <param name="MaxEmpruntsSimultanes">Livres empruntés en parallèle (emprunts en cours).</param>
/// <param name="MaxReservationsActives">Nombre de réservations <c>en_attente</c> en même temps (tous livres confondus).</param>
/// <param name="MaxAlertesActives">Alertes disponibilité non traitées en parallèle.</param>
/// <param name="DureeEmpruntJours">Durée d’un emprunt : date de retour prévu = aujourd’hui + N jours.</param>
public readonly record struct PlanLimites(
    string NomPlan,
    int MaxEmpruntsSimultanes,
    int MaxReservationsActives,
    int MaxAlertesActives,
    int DureeEmpruntJours)
{
    public static PlanLimites Gratuit { get; } = new(
        NomPlan: "Gratuit",
        MaxEmpruntsSimultanes: 2,
        MaxReservationsActives: 1,
        MaxAlertesActives: 2,
        DureeEmpruntJours: 14);

    public static PlanLimites Essentiel { get; } = new(
        NomPlan: "Essentiel (700 FCFA)",
        MaxEmpruntsSimultanes: 3,
        MaxReservationsActives: 2,
        MaxAlertesActives: 3,
        DureeEmpruntJours: 21);

    public static PlanLimites Standard { get; } = new(
        NomPlan: "Standard (1000 FCFA)",
        MaxEmpruntsSimultanes: 5,
        MaxReservationsActives: 4,
        MaxAlertesActives: 5,
        DureeEmpruntJours: 21);

    public static PlanLimites Annuel { get; } = new(
        NomPlan: "Premium annuel (5000 FCFA)",
        MaxEmpruntsSimultanes: 10,
        MaxReservationsActives: 8,
        MaxAlertesActives: 10,
        DureeEmpruntJours: 30);
}
