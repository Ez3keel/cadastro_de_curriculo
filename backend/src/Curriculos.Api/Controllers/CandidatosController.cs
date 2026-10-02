using Curriculos.Api.Dtos;
using Curriculos.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Curriculos.Api.Controllers;

/// <summary>
/// Cadastro e consulta de candidatos.
/// </summary>
[ApiController]
[Route("api/candidatos")]
[Produces("application/json")]
public class CandidatosController : ControllerBase
{
    private readonly ICandidatoService _candidatoService;
    private readonly IValidator<CandidatoRequest> _validator;

    public CandidatosController(ICandidatoService candidatoService, IValidator<CandidatoRequest> validator)
    {
        _candidatoService = candidatoService;
        _validator = validator;
    }

    /// <summary>
    /// Cadastra um novo candidato.
    /// </summary>
    /// <remarks>
    /// É o mesmo cadastro feito pelo formulário do frontend (manual ou preenchido a partir de um PDF).
    ///
    /// **Como usar:** clique em "Try it out", edite o JSON e clique em "Execute".
    ///
    /// - Obrigatórios: `nomeCompleto` e `email`.
    /// - Opcionais: `telefone`, `areaInteresse` e `resumoProfissional`.
    /// - Espaços no início e no fim dos campos são removidos antes de validar.
    /// - O e-mail é salvo em minúsculas e precisa ser único (`MARIA@x.com` e `maria@x.com` são o mesmo e-mail).
    /// - O `id` (Guid) é gerado pela API; não precisa ser enviado.
    /// </remarks>
    /// <response code="201">Candidato criado. O corpo traz o `id` gerado e o cabeçalho `Location` aponta para a consulta dele.</response>
    /// <response code="400">Dados inválidos (campo obrigatório vazio, e-mail inválido ou texto acima do limite). Traz a lista de erros por campo.</response>
    /// <response code="409">Já existe um candidato com este e-mail.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CandidatoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CandidatoResponse>> Criar(CandidatoRequest request)
    {
        TrimCampos(request);

        var resultadoValidacao = await _validator.ValidateAsync(request);
        if (!resultadoValidacao.IsValid)
        {
            foreach (var erro in resultadoValidacao.Errors)
            {
                ModelState.AddModelError(erro.PropertyName, erro.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        var candidato = await _candidatoService.CriarAsync(request);
        return CreatedAtAction(nameof(ObterPorId), new { id = candidato.Id }, candidato);
    }

    /// <summary>
    /// Lista todos os candidatos.
    /// </summary>
    /// <remarks>
    /// Não recebe parâmetros: basta clicar em "Try it out" e "Execute".
    /// Devolve id, nome, e-mail, área de interesse e data de cadastro, do mais recente para o mais antigo.
    /// Para ver todos os campos de um candidato, use a consulta por id.
    /// </remarks>
    /// <response code="200">Lista de candidatos (vazia se ainda não houver nenhum).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CandidatoListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CandidatoListItemResponse>>> Listar()
    {
        var candidatos = await _candidatoService.ListarAsync();
        return Ok(candidatos);
    }

    /// <summary>
    /// Consulta os detalhes de um candidato.
    /// </summary>
    /// <remarks>
    /// **Como usar:** copie o `id` de um candidato (da resposta do cadastro ou da listagem), cole no campo `id` e clique em "Execute".
    /// Devolve todos os campos, incluindo telefone e resumo profissional.
    /// </remarks>
    /// <param name="id">Identificador (Guid) do candidato, por exemplo `3fa85f64-5717-4562-b3fc-2c963f66afa6`.</param>
    /// <response code="200">Candidato encontrado.</response>
    /// <response code="404">Não existe candidato com esse id.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CandidatoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CandidatoResponse>> ObterPorId(Guid id)
    {
        var candidato = await _candidatoService.ObterPorIdAsync(id);
        return candidato is null ? NotFound() : Ok(candidato);
    }

    private static void TrimCampos(CandidatoRequest request)
    {
        request.NomeCompleto = request.NomeCompleto.Trim();
        request.Email = request.Email.Trim();
        request.Telefone = request.Telefone?.Trim();
        request.AreaInteresse = request.AreaInteresse?.Trim();
        request.ResumoProfissional = request.ResumoProfissional?.Trim();
    }
}
