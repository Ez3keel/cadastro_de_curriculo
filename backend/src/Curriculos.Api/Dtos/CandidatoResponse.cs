namespace Curriculos.Api.Dtos;

public record CandidatoResponse(
    int Id,
    string NomeCompleto,
    string Email,
    string? Telefone,
    string? AreaInteresse,
    string? ResumoProfissional,
    DateTime CriadoEm);
