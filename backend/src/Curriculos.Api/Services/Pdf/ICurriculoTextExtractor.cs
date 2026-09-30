namespace Curriculos.Api.Services.Pdf;

public interface ICurriculoTextExtractor
{
    string ExtrairTexto(byte[] conteudoPdf);
}
