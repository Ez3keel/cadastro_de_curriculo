using Curriculos.Api.Services.Pdf;

namespace Curriculos.Api.Tests.Parsers;

public class PdfPigTextExtractorTests
{
    private readonly PdfPigTextExtractor _extractor = new();

    [Fact]
    public void ExtrairTexto_nao_deve_misturar_colunas_diferentes_na_mesma_linha()
    {
        var conteudo = File.ReadAllBytes(Path.Combine("Fixtures", "curriculo-duas-colunas.pdf"));

        var texto = _extractor.ExtrairTexto(conteudo);
        var nomeExtraido = new CurriculoParser().Parse(texto).NomeCompleto;

        // O PDF de teste tem "CONTATO" na barra lateral e "Mariana Costa" no conteúdo
        // principal, ambos na mesma altura visual, mas em colunas diferentes.
        Assert.Equal("Mariana Costa", nomeExtraido);
    }
}
