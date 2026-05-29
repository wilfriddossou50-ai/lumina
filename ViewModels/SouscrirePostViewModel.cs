using System.ComponentModel.DataAnnotations;

namespace GestionBibliotheque.ViewModels;

public class SouscrirePostViewModel
{
    [Required]
    public int IdType { get; set; }

    public string? ModePaiement { get; set; }
}

