namespace Curriculos.Api.Dtos;

public record CandidatoListItemResponse(
    Guid Id,
    string NomeCompleto,
    string Email,
    string? AreaInteresse,
    DateTime CriadoEm);
