using System.Text.RegularExpressions;

namespace Curriculos.Api.Services.Pdf;

public partial class CurriculoParser
{
    private static readonly HashSet<string> TitulosIgnorados = new(StringComparer.OrdinalIgnoreCase)
    {
        "curriculo",
        "currículo",
        "curriculum vitae",
        "resumo",
        "resumo profissional",
        "cv",
    };

    public CurriculoExtraido Parse(string texto)
    {
        return new CurriculoExtraido(
            ExtrairNome(texto),
            ExtrairEmail(texto),
            ExtrairTelefone(texto));
    }

    private static string? ExtrairEmail(string texto)
    {
        var match = EmailRegex().Match(texto);
        return match.Success ? match.Value : null;
    }

    private static string? ExtrairTelefone(string texto)
    {
        var match = TelefoneRegex().Match(texto);
        return match.Success ? match.Value.Trim() : null;
    }

    private static string? ExtrairNome(string texto)
    {
        var linhas = texto.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var linha in linhas)
        {
            var linhaLimpa = linha.Trim();

            if (linhaLimpa.Length == 0 || TitulosIgnorados.Contains(linhaLimpa))
            {
                continue;
            }

            if (NomeRegex().IsMatch(linhaLimpa))
            {
                return linhaLimpa;
            }
        }

        return null;
    }

    [GeneratedRegex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"(?:\+55\s?)?\(?\d{2}\)?\s?9?\d{4}-?\d{4}")]
    private static partial Regex TelefoneRegex();

    [GeneratedRegex(@"^[a-zA-ZÀ-ÖØ-öø-ÿ]+(\s[a-zA-ZÀ-ÖØ-öø-ÿ]+){1,5}$")]
    private static partial Regex NomeRegex();
}
