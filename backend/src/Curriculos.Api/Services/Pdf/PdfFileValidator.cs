using Curriculos.Api.Exceptions;

namespace Curriculos.Api.Services.Pdf;

public static class PdfFileValidator
{
    public const long TamanhoMaximoEmBytes = 5 * 1024 * 1024;

    private static readonly byte[] AssinaturaPdf = "%PDF-"u8.ToArray();

    public static void Validar(string? nomeArquivo, long tamanho, byte[] conteudo)
    {
        if (string.IsNullOrWhiteSpace(nomeArquivo) || tamanho == 0)
        {
            throw new ArquivoInvalidoException();
        }

        if (tamanho > TamanhoMaximoEmBytes)
        {
            throw new ArquivoInvalidoException();
        }

        if (!Path.GetExtension(nomeArquivo).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArquivoInvalidoException();
        }

        if (conteudo.Length < AssinaturaPdf.Length ||
            !conteudo.AsSpan(0, AssinaturaPdf.Length).SequenceEqual(AssinaturaPdf))
        {
            throw new ArquivoInvalidoException();
        }
    }
}
