using System.ComponentModel.DataAnnotations;

namespace GestionBibliotheque.ViewModels;

public class ProfilViewModel
{
    public int IdUtilisateur { get; set; }
    public int? IdMembre { get; set; }

    [Required(ErrorMessage = "Le login est obligatoire")]
    [StringLength(20, ErrorMessage = "Maximum 20 caracteres")]
    public string Login { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Maximum 20 caracteres")]
    public string? Nom { get; set; }

    [StringLength(20, ErrorMessage = "Maximum 20 caracteres")]
    public string? Prenom { get; set; }

    public string? Sexe { get; set; }

    [StringLength(15, ErrorMessage = "Maximum 15 caracteres")]
    public string? Telephone { get; set; }

    [EmailAddress(ErrorMessage = "Email invalide")]
    [StringLength(100, ErrorMessage = "Maximum 100 caracteres")]
    public string? Mail { get; set; }

    [StringLength(200, ErrorMessage = "Maximum 200 caracteres")]
    public string? Adresse { get; set; }

    public string? Statut { get; set; }
    public DateOnly? DateInscription { get; set; }

    public string DisplayName => string.IsNullOrWhiteSpace(Prenom)
        ? (Nom ?? Login)
        : $"{Prenom} {Nom}";
}

public class ProfilPageViewModel
{
    public ProfilViewModel Profile { get; set; } = new();
    public ChangePasswordViewModel Password { get; set; } = new();

    public bool IsAdmin { get; set; }
    public string DashboardController { get; set; } = "MemberDashboard";
    public string DashboardLabel { get; set; } = "Retour au tableau de bord";
    public string DisplayName { get; set; } = "Utilisateur";
    public string Initials { get; set; } = "?";
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Le mot de passe actuel est requis")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Minimum 6 caracteres")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmation est requise")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
