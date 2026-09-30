using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Curriculos.Api.Services.Pdf;

public class PdfPigTextExtractor : ICurriculoTextExtractor
{
    // Palavras na mesma linha visual têm o Top da bounding box um pouco diferente
    // (varia com a altura de cada glifo), por isso as linhas são agrupadas com
    // tolerância em vez de comparar o valor exato.
    private const double ToleranciaMesmaLinha = 5.0;

    public string ExtrairTexto(byte[] conteudoPdf)
    {
        using var stream = new MemoryStream(conteudoPdf);
        using var documento = PdfDocument.Open(stream);

        var texto = new StringBuilder();
        foreach (var pagina in documento.GetPages())
        {
            foreach (var linha in AgruparEmLinhas(pagina.GetWords()))
            {
                texto.AppendLine(linha);
            }
        }

        return texto.ToString();
    }

    private static IEnumerable<string> AgruparEmLinhas(IEnumerable<Word> palavras)
    {
        var palavrasOrdenadas = palavras.OrderByDescending(p => p.BoundingBox.Top).ToList();

        var linhaAtual = new List<Word>();
        double? topDaLinhaAtual = null;

        foreach (var palavra in palavrasOrdenadas)
        {
            if (topDaLinhaAtual is null || Math.Abs(topDaLinhaAtual.Value - palavra.BoundingBox.Top) <= ToleranciaMesmaLinha)
            {
                linhaAtual.Add(palavra);
                topDaLinhaAtual ??= palavra.BoundingBox.Top;
            }
            else
            {
                yield return MontarLinha(linhaAtual);
                linhaAtual = [palavra];
                topDaLinhaAtual = palavra.BoundingBox.Top;
            }
        }

        if (linhaAtual.Count > 0)
        {
            yield return MontarLinha(linhaAtual);
        }
    }

    private static string MontarLinha(List<Word> palavrasDaLinha) =>
        string.Join(' ', palavrasDaLinha.OrderBy(p => p.BoundingBox.Left).Select(p => p.Text));
}
