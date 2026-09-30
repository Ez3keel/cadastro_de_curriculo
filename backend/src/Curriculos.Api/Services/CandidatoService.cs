using Curriculos.Api.Data;
using Curriculos.Api.Dtos;
using Curriculos.Api.Exceptions;
using Curriculos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Curriculos.Api.Services;

public class CandidatoService : ICandidatoService
{
    private readonly AppDbContext _context;

    public CandidatoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CandidatoResponse> CriarAsync(CandidatoRequest request)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        var emailJaCadastrado = await _context.Candidatos
            .AnyAsync(c => c.Email == emailNormalizado);

        if (emailJaCadastrado)
        {
            throw new EmailDuplicadoException();
        }

        var candidato = new Candidato
        {
            Id = Guid.NewGuid(),
            NomeCompleto = request.NomeCompleto.Trim(),
            Email = emailNormalizado,
            Telefone = request.Telefone?.Trim(),
            AreaInteresse = request.AreaInteresse?.Trim(),
            ResumoProfissional = request.ResumoProfissional?.Trim(),
            CriadoEm = DateTime.UtcNow,
        };

        _context.Candidatos.Add(candidato);
        await _context.SaveChangesAsync();

        return ParaResponse(candidato);
    }

    public async Task<IReadOnlyList<CandidatoListItemResponse>> ListarAsync()
    {
        return await _context.Candidatos
            .OrderByDescending(c => c.CriadoEm)
            .Select(c => new CandidatoListItemResponse(c.Id, c.NomeCompleto, c.Email, c.AreaInteresse, c.CriadoEm))
            .ToListAsync();
    }

    public async Task<CandidatoResponse?> ObterPorIdAsync(Guid id)
    {
        var candidato = await _context.Candidatos.FindAsync(id);
        return candidato is null ? null : ParaResponse(candidato);
    }

    private static CandidatoResponse ParaResponse(Candidato candidato) => new(
        candidato.Id,
        candidato.NomeCompleto,
        candidato.Email,
        candidato.Telefone,
        candidato.AreaInteresse,
        candidato.ResumoProfissional,
        candidato.CriadoEm);
}
