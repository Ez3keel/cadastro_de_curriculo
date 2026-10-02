namespace Curriculos.Api.Dtos;

/// <summary>
/// Dados para cadastrar um candidato.
/// </summary>
public class CandidatoRequest
{
    /// <summary>Nome completo. Obrigatório, até 150 caracteres.</summary>
    /// <example>Maria Souza</example>
    public string NomeCompleto { get; set; } = string.Empty;

    /// <summary>E-mail. Obrigatório, formato válido, até 254 caracteres e único entre os candidatos.</summary>
    /// <example>maria.souza@exemplo.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>Telefone. Opcional, até 20 caracteres, em qualquer formato brasileiro comum.</summary>
    /// <example>(41) 99999-9999</example>
    public string? Telefone { get; set; }

    /// <summary>Área ou cargo de interesse. Opcional, até 100 caracteres.</summary>
    /// <example>Desenvolvimento Backend</example>
    public string? AreaInteresse { get; set; }

    /// <summary>Resumo profissional. Opcional, até 2000 caracteres.</summary>
    /// <example>Desenvolvedora com 3 anos de experiência em .NET e SQL Server.</example>
    public string? ResumoProfissional { get; set; }
}
