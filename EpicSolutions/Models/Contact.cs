using System.ComponentModel.DataAnnotations;

namespace EpicSolutions.Models
{
    public class Contact
    {
        [Required]
        [Display(Name = "Nombre", Prompt = "Nombre")]
        [DataType(DataType.Text)]
        public string Name { get; set; } = null!;
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Correo electrónico", Prompt = "Correo electrónico")]
        public string Email { get; set; } = null!;
        [DataType(DataType.Text)]
        [Display(Name = "Asunto", Prompt = "Asunto")]
        [Required]
        public string Subject { get; set; } = null!;
        [DataType(DataType.MultilineText)]
        [Display(Name = "Escríbenos algo", Prompt = "Escríbenos algo")]
        [Required]
        public string Message { get; set; } = null!;
        public bool IsValid { get; set; }
    }
}
