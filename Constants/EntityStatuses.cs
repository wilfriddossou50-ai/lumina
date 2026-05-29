namespace GestionBibliotheque.Constants;

public static class EmpruntStatuses
{
    public const string EnCours = "en_cours";
}

public static class ReservationStatuses
{
    public const string EnAttente = "en_attente";
    public const string Disponible = "disponible";
    public const string Honoree = "honoree";
}

public static class AbonnementStatuses
{
    public const string Actif = "actif";
    public const string Inactif = "inactif";
}

public static class PaiementStatuses
{
    public const string Paye = "paye";
    public const string EnAttente = "en_attente";
    public const string Refuse = "refuse";
}

public static class LivreStatuses
{
    public const string Disponible = "disponible";
    public const string Indisponible = "indisponible";
}
