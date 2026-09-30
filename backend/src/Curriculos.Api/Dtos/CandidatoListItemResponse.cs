namespace Curriculos.Api.Dtos;

public record CandidatoListItemResponse(
    int Id,
    string NomeCompleto,
    string Email,
    string? AreaInteresse,
    DateTime CriadoEm);
