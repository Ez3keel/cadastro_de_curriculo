using Curriculos.Api.Dtos;
using Curriculos.Api.Validators;

namespace Curriculos.Api.Tests.Validators;

public class CandidatoRequestValidatorTests
{
    private readonly CandidatoRequestValidator _validator = new();

    private static CandidatoRequest CriarRequestValido() => new()
    {
        NomeCompleto = "Maria Souza",
        Email = "maria@exemplo.com",
        Telefone = "(41) 99999-9999",
        AreaInteresse = "Desenvolvimento",
        ResumoProfissional = "Resumo de teste.",
    };

    [Fact]
    public void Deve_ser_valido_quando_todos_os_campos_obrigatorios_estao_preenchidos()
    {
        var resultado = _validator.Validate(CriarRequestValido());

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Deve_falhar_quando_nome_completo_esta_vazio()
    {
        var request = CriarRequestValido();
        request.NomeCompleto = "";

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CandidatoRequest.NomeCompleto));
    }

    [Fact]
    public void Deve_falhar_quando_email_esta_vazio()
    {
        var request = CriarRequestValido();
        request.Email = "";

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CandidatoRequest.Email));
    }

    [Fact]
    public void Deve_falhar_quando_email_tem_formato_invalido()
    {
        var request = CriarRequestValido();
        request.Email = "nao-e-um-email";

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CandidatoRequest.Email));
    }

    [Fact]
    public void Deve_falhar_quando_nome_completo_excede_150_caracteres()
    {
        var request = CriarRequestValido();
        request.NomeCompleto = new string('a', 151);

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CandidatoRequest.NomeCompleto));
    }

    [Fact]
    public void Deve_falhar_quando_resumo_profissional_excede_2000_caracteres()
    {
        var request = CriarRequestValido();
        request.ResumoProfissional = new string('a', 2001);

        var resultado = _validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CandidatoRequest.ResumoProfissional));
    }

    [Fact]
    public void Deve_ser_valido_quando_campos_opcionais_nao_sao_informados()
    {
        var request = CriarRequestValido();
        request.Telefone = null;
        request.AreaInteresse = null;
        request.ResumoProfissional = null;

        var resultado = _validator.Validate(request);

        Assert.True(resultado.IsValid);
    }
}
