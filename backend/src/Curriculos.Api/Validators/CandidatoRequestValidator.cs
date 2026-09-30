using Curriculos.Api.Dtos;
using FluentValidation;

namespace Curriculos.Api.Validators;

public class CandidatoRequestValidator : AbstractValidator<CandidatoRequest>
{
    public CandidatoRequestValidator()
    {
        RuleFor(c => c.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MaximumLength(150).WithMessage("O nome completo deve ter no máximo 150 caracteres.");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("Informe um e-mail válido.")
            .MaximumLength(254).WithMessage("O e-mail deve ter no máximo 254 caracteres.");

        RuleFor(c => c.Telefone)
            .MaximumLength(20).WithMessage("O telefone deve ter no máximo 20 caracteres.");

        RuleFor(c => c.AreaInteresse)
            .MaximumLength(100).WithMessage("A área de interesse deve ter no máximo 100 caracteres.");

        RuleFor(c => c.ResumoProfissional)
            .MaximumLength(2000).WithMessage("O resumo profissional deve ter no máximo 2000 caracteres.");
    }
}
