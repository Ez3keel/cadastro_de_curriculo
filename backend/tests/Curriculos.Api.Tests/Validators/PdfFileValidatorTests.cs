using Curriculos.Api.Exceptions;
using Curriculos.Api.Services.Pdf;

namespace Curriculos.Api.Tests.Validators;

public class PdfFileValidatorTests
{
    private static byte[] ConteudoPdfValido() => "%PDF-1.4\n%conteúdo de teste"u8.ToArray();

    [Fact]
    public void Deve_aceitar_arquivo_pdf_valido()
    {
        var exception = Record.Exception(() =>
            PdfFileValidator.Validar("curriculo.pdf", ConteudoPdfValido().Length, ConteudoPdfValido()));

        Assert.Null(exception);
    }

    [Fact]
    public void Deve_rejeitar_extensao_diferente_de_pdf()
    {
        Assert.Throws<ArquivoInvalidoException>(() =>
            PdfFileValidator.Validar("curriculo.docx", ConteudoPdfValido().Length, ConteudoPdfValido()));
    }

    [Fact]
    public void Deve_rejeitar_arquivo_com_assinatura_invalida()
    {
        var conteudoInvalido = "não é um pdf"u8.ToArray();

        Assert.Throws<ArquivoInvalidoException>(() =>
            PdfFileValidator.Validar("curriculo.pdf", conteudoInvalido.Length, conteudoInvalido));
    }

    [Fact]
    public void Deve_rejeitar_arquivo_acima_de_5mb()
    {
        var conteudo = ConteudoPdfValido();
        var tamanhoAcimaDoLimite = PdfFileValidator.TamanhoMaximoEmBytes + 1;

        Assert.Throws<ArquivoInvalidoException>(() =>
            PdfFileValidator.Validar("curriculo.pdf", tamanhoAcimaDoLimite, conteudo));
    }

    [Fact]
    public void Deve_rejeitar_quando_arquivo_nao_e_informado()
    {
        Assert.Throws<ArquivoInvalidoException>(() =>
            PdfFileValidator.Validar(null, 0, Array.Empty<byte>()));
    }
}
