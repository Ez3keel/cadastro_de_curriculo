using Curriculos.Api.Dtos;
using Curriculos.Api.Exceptions;
using Curriculos.Api.Services.Pdf;
using Microsoft.AspNetCore.Mvc;

namespace Curriculos.Api.Controllers;

/// <summary>
/// Leitura de currículos em PDF.
/// </summary>
[ApiController]
[Route("api/curriculos")]
[Produces("application/json")]
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

    /// <summary>
    /// Extrai nome, e-mail e telefone de um currículo em PDF, sem salvar nada.
    /// </summary>
    /// <remarks>
    /// Serve para preencher o formulário de cadastro: o usuário revisa os dados e só então salva
    /// pelo cadastro de candidato (`POST /api/candidatos`). Esta rota **não grava** nada no banco.
    ///
    /// **Como usar:** clique em "Try it out", escolha o arquivo no campo `arquivo` e clique em "Execute".
    ///
    /// - O arquivo precisa ser um PDF (extensão `.pdf`) de até 5 MB, com texto selecionável.
    /// - PDFs escaneados (só imagem) não são lidos: nesse caso o cadastro deve ser manual.
    /// - Campos que não foram encontrados vêm como `null` e aparecem na lista `camposNaoEncontrados`.
    /// - Há PDFs de exemplo na pasta `docs/` do repositório.
    /// </remarks>
    /// <param name="arquivo">Arquivo PDF do currículo (campo multipart `arquivo`).</param>
    /// <response code="200">Dados extraídos (campos não encontrados vêm nulos).</response>
    /// <response code="400">Arquivo ausente ou inválido: não é PDF ou passa de 5 MB.</response>
    /// <response code="422">Não foi possível ler texto do PDF (por exemplo, PDF escaneado). Preencha os dados manualmente.</response>
    [HttpPost("extrair")]
    [ProducesResponseType(typeof(CurriculoExtracaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
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
