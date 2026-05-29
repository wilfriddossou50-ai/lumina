namespace GestionBibliotheque.Services;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsMembre { get; }
    int? UtilisateurId { get; }
    int? MembreId { get; }
    string? Role { get; }
    string? Login { get; }
    string DisplayName { get; }
    string Initials { get; }

    void SignIn(int utilisateurId, string role, string login, int? membreId, string displayName);
    void SignOut();
}
