namespace Curriculos.Api.Dtos;

public record CurriculoExtracaoResponse(
    string? NomeCompleto,
    string? Email,
    string? Telefone,
    List<string> CamposNaoEncontrados);
