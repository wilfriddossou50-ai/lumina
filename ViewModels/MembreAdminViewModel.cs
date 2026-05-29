using System.ComponentModel.DataAnnotations;

namespace GestionBibliotheque.ViewModels;

public class MembreAdminViewModel
{
    public int? IdMembre { get; set; }
    public int? IdUtilisateur { get; set; }

    [Required(ErrorMessage = "Le prénom est obligatoire")]
    [StringLength(20, ErrorMessage = "Maximum 20 caractères")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [StringLength(20, ErrorMessage = "Maximum 20 caractères")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le sexe est obligatoire")]
    public string Sexe { get; set; } = string.Empty;

    [StringLength(15, ErrorMessage = "Maximum 15 caractères")]
    public string? Telephone { get; set; }

    [Required(ErrorMessage = "L'email est obligatoire")]
    [EmailAddress(ErrorMessage = "Email invalide")]
    [StringLength(100, ErrorMessage = "Maximum 100 caractères")]
    public string Mail { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Maximum 200 caractères")]
    public string? Adresse { get; set; }

    [Required(ErrorMessage = "Le statut est obligatoire")]
    public string Statut { get; set; } = "actif";

    [Required(ErrorMessage = "L'identifiant est obligatoire")]
    [StringLength(20, ErrorMessage = "Maximum 20 caractères")]
    public string Login { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Minimum 6 caractères")]
    public string? Password { get; set; }

    public bool AccepterConditions { get; set; } = true;
    public DateOnly? DateInscription { get; set; }
}
