using Curriculos.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Curriculos.Api.Data.Configurations;

public class CandidatoConfiguration : IEntityTypeConfiguration<Candidato>
{
    public void Configure(EntityTypeBuilder<Candidato> builder)
    {
        builder.ToTable("Candidatos");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.NomeCompleto)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.Property(c => c.Telefone)
            .HasMaxLength(20);

        builder.Property(c => c.AreaInteresse)
            .HasMaxLength(100);

        builder.Property(c => c.ResumoProfissional)
            .HasMaxLength(2000);

        builder.Property(c => c.CriadoEm)
            .IsRequired();
    }
}
