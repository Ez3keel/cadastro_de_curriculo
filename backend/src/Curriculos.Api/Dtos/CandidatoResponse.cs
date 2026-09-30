namespace Curriculos.Api.Dtos;

public record CandidatoResponse(
    Guid Id,
    string NomeCompleto,
    string Email,
    string? Telefone,
    string? AreaInteresse,
    string? ResumoProfissional,
    DateTime CriadoEm);
