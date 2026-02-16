using fin_api.Models.Validations;
using FluentValidation.Results;

namespace fin_api.Models;

public class Usuario : Entity
{
    public string Nome { get; set; }
    public ICollection<Transacao> transacaos { get; set; } = new List<Transacao>();
    public ValidationResult ValidationResult { get; private set; }

    public bool EhValido()
    {
        ValidationResult = new UsuarioValidation().Validate(this);
        return ValidationResult.IsValid;
    }
}
