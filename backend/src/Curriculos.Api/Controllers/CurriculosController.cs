using Curriculos.Api.Dtos;
using Curriculos.Api.Exceptions;
using Curriculos.Api.Services.Pdf;
using Microsoft.AspNetCore.Mvc;

namespace Curriculos.Api.Controllers;

[ApiController]
[Route("api/curriculos")]
public class CurriculosController : ControllerBase
{
    private const int TamanhoMinimoTextoValido = 50;

    private readonly ICurriculoTextExtractor _extractor;
    private readonly CurriculoParser _parser;
    private readonly ILogger<CurriculosController> _logger;

    public CurriculosController(ICurriculoTextExtractor extractor, CurriculoParser parser, ILogger<CurriculosController> logger)
    {
        _extractor = extractor;
        _parser = parser;
        _logger = logger;
    }

    [HttpPost("extrair")]
    [RequestSizeLimit(PdfFileValidator.TamanhoMaximoEmBytes)]
    public async Task<ActionResult<CurriculoExtracaoResponse>> Extrair(IFormFile? arquivo)
    {
        if (arquivo is null)
        {
            throw new ArquivoInvalidoException();
        }

        byte[] conteudo;
        using (var memoryStream = new MemoryStream())
        {
            await arquivo.CopyToAsync(memoryStream);
            conteudo = memoryStream.ToArray();
        }

        PdfFileValidator.Validar(arquivo.FileName, arquivo.Length, conteudo);

        _logger.LogInformation(
            "Recebido currículo {NomeArquivo} ({Tamanho} bytes) para extração.",
            arquivo.FileName,
            arquivo.Length);

        string texto;
        try
        {
            texto = _extractor.ExtrairTexto(conteudo);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao extrair texto do PDF {NomeArquivo}.", arquivo.FileName);
            throw new FalhaLeituraPdfException();
        }

        if (texto.Length < TamanhoMinimoTextoValido)
        {
            throw new FalhaLeituraPdfException();
        }

        var extraido = _parser.Parse(texto);

        var camposNaoEncontrados = new List<string>();
        if (extraido.NomeCompleto is null) camposNaoEncontrados.Add("nomeCompleto");
        if (extraido.Email is null) camposNaoEncontrados.Add("email");
        if (extraido.Telefone is null) camposNaoEncontrados.Add("telefone");

        return Ok(new CurriculoExtracaoResponse(extraido.NomeCompleto, extraido.Email, extraido.Telefone, camposNaoEncontrados));
    }
}
