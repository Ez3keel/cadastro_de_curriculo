using Curriculos.Api.Dtos;

namespace Curriculos.Api.Services;

public interface ICandidatoService
{
    Task<CandidatoResponse> CriarAsync(CandidatoRequest request);
    Task<IReadOnlyList<CandidatoListItemResponse>> ListarAsync();
    Task<CandidatoResponse?> ObterPorIdAsync(int id);
}
