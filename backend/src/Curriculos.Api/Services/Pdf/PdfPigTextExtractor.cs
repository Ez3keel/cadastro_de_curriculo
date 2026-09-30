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

    // Em layouts de duas colunas (ex.: barra lateral e conteúdo principal), palavras de
    // colunas diferentes podem cair na mesma altura. Um espaço horizontal bem maior que o
    // espaço normal entre palavras indica uma quebra de coluna, não uma continuação da linha.
    private const double ToleranciaEspacoEntrePalavras = 40.0;

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
                foreach (var subLinha in SepararPorColuna(linhaAtual))
                {
                    yield return subLinha;
                }

                linhaAtual = [palavra];
                topDaLinhaAtual = palavra.BoundingBox.Top;
            }
        }

        if (linhaAtual.Count > 0)
        {
            foreach (var subLinha in SepararPorColuna(linhaAtual))
            {
                yield return subLinha;
            }
        }
    }

    private static IEnumerable<string> SepararPorColuna(List<Word> palavrasDaLinha)
    {
        var ordenadas = palavrasDaLinha.OrderBy(p => p.BoundingBox.Left).ToList();

        var grupoAtual = new List<Word> { ordenadas[0] };

        for (var i = 1; i < ordenadas.Count; i++)
        {
            var espaco = ordenadas[i].BoundingBox.Left - ordenadas[i - 1].BoundingBox.Right;
            if (espaco > ToleranciaEspacoEntrePalavras)
            {
                yield return string.Join(' ', grupoAtual.Select(p => p.Text));
                grupoAtual = [];
            }

            grupoAtual.Add(ordenadas[i]);
        }

        yield return string.Join(' ', grupoAtual.Select(p => p.Text));
    }
}
