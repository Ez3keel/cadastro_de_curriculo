using Curriculos.Api.Data;
using Curriculos.Api.Dtos;
using Curriculos.Api.Exceptions;
using Curriculos.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Curriculos.Api.Tests.Services;

public class CandidatoServiceTests
{
    private static AppDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static CandidatoRequest CriarRequest(string email = "maria@exemplo.com") => new()
    {
        NomeCompleto = "Maria Souza",
        Email = email,
        Telefone = "(41) 99999-9999",
        AreaInteresse = "Desenvolvimento",
        ResumoProfissional = "Resumo de teste.",
    };

    [Fact]
    public async Task CriarAsync_deve_salvar_candidato_com_email_normalizado()
    {
        await using var context = CriarContexto();
        var service = new CandidatoService(context);

        var resultado = await service.CriarAsync(CriarRequest("  Maria@Exemplo.com  "));

        Assert.Equal("maria@exemplo.com", resultado.Email);
    }

    [Fact]
    public async Task CriarAsync_deve_lancar_EmailDuplicadoException_quando_email_ja_existe()
    {
        await using var context = CriarContexto();
        var service = new CandidatoService(context);

        await service.CriarAsync(CriarRequest("maria@exemplo.com"));

        await Assert.ThrowsAsync<EmailDuplicadoException>(
            () => service.CriarAsync(CriarRequest("MARIA@EXEMPLO.COM")));
    }

    [Fact]
    public async Task ListarAsync_deve_retornar_candidatos_do_mais_recente_para_o_mais_antigo()
    {
        await using var context = CriarContexto();
        var service = new CandidatoService(context);

        await service.CriarAsync(CriarRequest("primeiro@exemplo.com"));
        await service.CriarAsync(CriarRequest("segundo@exemplo.com"));

        var resultado = await service.ListarAsync();

        Assert.Equal(2, resultado.Count);
        Assert.Equal("segundo@exemplo.com", resultado[0].Email);
        Assert.Equal("primeiro@exemplo.com", resultado[1].Email);
    }

    [Fact]
    public async Task ObterPorIdAsync_deve_retornar_null_quando_candidato_nao_existe()
    {
        await using var context = CriarContexto();
        var service = new CandidatoService(context);

        var resultado = await service.ObterPorIdAsync(999);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterPorIdAsync_deve_retornar_candidato_quando_ele_existe()
    {
        await using var context = CriarContexto();
        var service = new CandidatoService(context);

        var criado = await service.CriarAsync(CriarRequest());

        var resultado = await service.ObterPorIdAsync(criado.Id);

        Assert.NotNull(resultado);
        Assert.Equal(criado.Email, resultado!.Email);
    }
}
