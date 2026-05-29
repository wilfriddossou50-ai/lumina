using GestionBibliotheque.Constants;
using Microsoft.AspNetCore.Http;

namespace GestionBibliotheque.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession? Session => _httpContextAccessor.HttpContext?.Session;

    public bool IsAuthenticated => UtilisateurId.HasValue && !string.IsNullOrWhiteSpace(Role);
    public bool IsAdmin => Role == ApplicationRoles.Admin;
    public bool IsMembre => Role == ApplicationRoles.Membre;
    public int? UtilisateurId => Session?.GetInt32(SessionKeys.UtilisateurId);
    public int? MembreId => Session?.GetInt32(SessionKeys.MembreId);
    public string? Role => Session?.GetString(SessionKeys.UserRole);
    public string? Login => Session?.GetString(SessionKeys.UserLogin);
    public string DisplayName =>
        Session?.GetString(SessionKeys.UserDisplayName) ??
        Login ??
        "Utilisateur";

    public string Initials
    {
        get
        {
            var parts = DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                return $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant();
            }

            if (parts.Length == 1 && parts[0].Length > 0)
            {
                return parts[0][0].ToString().ToUpperInvariant();
            }

            return "?";
        }
    }

    public void SignIn(int utilisateurId, string role, string login, int? membreId, string displayName)
    {
        if (Session == null)
        {
            return;
        }

        Session.SetInt32(SessionKeys.UtilisateurId, utilisateurId);
        Session.SetString(SessionKeys.UserRole, role);
        Session.SetString(SessionKeys.UserLogin, login);
        Session.SetString(SessionKeys.UserDisplayName, displayName);

        if (membreId.HasValue)
        {
            Session.SetInt32(SessionKeys.MembreId, membreId.Value);
        }
        else
        {
            Session.Remove(SessionKeys.MembreId);
        }
    }

    public void SignOut()
    {
        Session?.Clear();
    }
}
