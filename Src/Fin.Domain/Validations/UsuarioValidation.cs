using Fin.Domain.Models;
using FluentValidation;

namespace Fin.Domain.Validations
{
    public class UsuarioValidation : AbstractValidator<Usuario>
    {
        public UsuarioValidation()
        {
            RuleFor(u => u.Id)
                .NotEmpty()
                .WithMessage("O Id do usuário é obrigatório");

            RuleFor(u => u.Nome)
                .NotEmpty()
                .WithMessage("O nome do usuário é obrigatório")
                .MinimumLength(2)
                .WithMessage("O nome deve ter pelo menos 2 caracteres")
                .MaximumLength(100)
                .WithMessage("O nome deve ter no máximo 100 caracteres");

           RuleFor(u => u.Nome)
                .NotEmpty()
                .WithMessage("O email do usuário é obrigatório")
                .MinimumLength(2)
                .WithMessage("O email deve ter pelo menos 2 caracteres")
                .MaximumLength(100)
                .WithMessage("O email deve ter no máximo 100 caracteres");
        }
    }
}
