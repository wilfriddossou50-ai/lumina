using System.ComponentModel.DataAnnotations;

namespace GestionBibliotheque.ViewModels;

public class InscriptionViewModel
{
    [Required(ErrorMessage = "Le prénom est obligatoire")]
    [StringLength(20, ErrorMessage = "Maximum 20 caractères")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [StringLength(10, ErrorMessage = "Maximum 10 caractères")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le sexe est obligatoire")]
    public string Sexe { get; set; } = string.Empty;

    [StringLength(15)]
    public string? Telephone { get; set; }

    [Required(ErrorMessage = "L'email est obligatoire")]
    [EmailAddress(ErrorMessage = "Email invalide")]
    [StringLength(100)]
    public string Mail { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage =         "Maximum 20 caractères")]
    public string? Adresse { get; set; }

    [Required(ErrorMessage = "L'identifiant est obligatoire")]
    [StringLength(20, ErrorMessage = "Maximum 20 caractères")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Minimum 6 caractères")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vous devez accepter les conditions")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Vous devez accepter les conditions")]
    public bool AccepterConditions { get; set; }
}
