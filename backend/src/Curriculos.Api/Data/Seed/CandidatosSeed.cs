using Curriculos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Curriculos.Api.Data.Seed;

public static class CandidatosSeed
{
    public static async Task ExecutarAsync(AppDbContext context)
    {
        if (await context.Candidatos.AnyAsync())
        {
            return;
        }

        context.Candidatos.AddRange(
            new Candidato
            {
                NomeCompleto = "Ana Beatriz Ferreira",
                Email = "ana.ferreira@exemplo.com",
                Telefone = "(41) 98888-1111",
                AreaInteresse = "Desenvolvimento Backend",
                ResumoProfissional = "Candidata fictícia gerada pelo seed, com interesse em vagas de backend .NET.",
                CriadoEm = DateTime.UtcNow,
            },
            new Candidato
            {
                NomeCompleto = "Bruno Costa Lima",
                Email = "bruno.lima@exemplo.com",
                Telefone = "(41) 97777-2222",
                AreaInteresse = "Suporte Técnico",
                ResumoProfissional = "Candidato fictício gerado pelo seed, com experiência em atendimento e suporte.",
                CriadoEm = DateTime.UtcNow,
            },
            new Candidato
            {
                NomeCompleto = "Carla Souza Martins",
                Email = "carla.martins@exemplo.com",
                Telefone = "(41) 96666-3333",
                AreaInteresse = "Análise de Dados",
                ResumoProfissional = "Candidata fictícia gerada pelo seed, com interesse em vagas de dados e BI.",
                CriadoEm = DateTime.UtcNow,
            });

        await context.SaveChangesAsync();
    }
}
