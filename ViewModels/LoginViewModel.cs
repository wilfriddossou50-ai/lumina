using System.ComponentModel.DataAnnotations;

namespace GestionBibliotheque.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "L'identifiant est obligatoire")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}