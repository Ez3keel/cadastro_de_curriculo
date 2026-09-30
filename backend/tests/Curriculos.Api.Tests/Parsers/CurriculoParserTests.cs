using Curriculos.Api.Services.Pdf;

namespace Curriculos.Api.Tests.Parsers;

public class CurriculoParserTests
{
    private readonly CurriculoParser _parser = new();

    [Fact]
    public void Deve_extrair_email_quando_presente_no_texto()
    {
        var texto = "Maria Souza\nEmail: maria@exemplo.com\nTelefone: (41) 99999-9999";

        var resultado = _parser.Parse(texto);

        Assert.Equal("maria@exemplo.com", resultado.Email);
    }

    [Fact]
    public void Deve_retornar_email_nulo_quando_nao_ha_email_no_texto()
    {
        var texto = "Maria Souza\nTelefone: (41) 99999-9999";

        var resultado = _parser.Parse(texto);

        Assert.Null(resultado.Email);
    }

    [Theory]
    [InlineData("Telefone: (41) 99999-9999", "(41) 99999-9999")]
    [InlineData("Contato: 41999999999", "41999999999")]
    [InlineData("Celular: +55 41 99999-9999", "+55 41 99999-9999")]
    [InlineData("Fixo: (41) 3333-4444", "(41) 3333-4444")]
    public void Deve_extrair_telefone_em_formatos_variados(string texto, string telefoneEsperado)
    {
        var resultado = _parser.Parse(texto);

        Assert.Equal(telefoneEsperado, resultado.Telefone);
    }

    [Fact]
    public void Deve_extrair_nome_da_primeira_linha_valida()
    {
        var texto = "Maria Souza da Silva\nDesenvolvedora Backend\nmaria@exemplo.com";

        var resultado = _parser.Parse(texto);

        Assert.Equal("Maria Souza da Silva", resultado.NomeCompleto);
    }

    [Fact]
    public void Deve_ignorar_titulo_curriculo_e_extrair_nome_da_proxima_linha()
    {
        var texto = "Currículo\nJoão Pereira\njoao@exemplo.com";

        var resultado = _parser.Parse(texto);

        Assert.Equal("João Pereira", resultado.NomeCompleto);
    }

    [Fact]
    public void Deve_ignorar_titulo_curriculum_vitae()
    {
        var texto = "Curriculum Vitae\nAna Paula Lima\nana@exemplo.com";

        var resultado = _parser.Parse(texto);

        Assert.Equal("Ana Paula Lima", resultado.NomeCompleto);
    }

    [Fact]
    public void Deve_retornar_todos_os_campos_nulos_quando_texto_vazio()
    {
        var resultado = _parser.Parse(string.Empty);

        Assert.Null(resultado.NomeCompleto);
        Assert.Null(resultado.Email);
        Assert.Null(resultado.Telefone);
    }
}
