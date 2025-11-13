using System.ComponentModel.DataAnnotations;

namespace fin_api.Models
{
    public class LoginUserViewModel
    {
        //[Required(ErrorMessage = "O campo {0} é obrigatório")]
        //[EmailAddress(ErrorMessage = "O campo {0} está em formato inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(100, ErrorMessage = "O campo precisa{0} ter entre {2} e {1} caracteres", MinimumLength = 6)]
        public string Password { get; set; }
    }
}
