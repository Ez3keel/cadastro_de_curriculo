using Curriculos.Api.Dtos;
using Curriculos.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Curriculos.Api.Controllers;

[ApiController]
[Route("api/candidatos")]
public class CandidatosController : ControllerBase
{
    private readonly ICandidatoService _candidatoService;
    private readonly IValidator<CandidatoRequest> _validator;

    public CandidatosController(ICandidatoService candidatoService, IValidator<CandidatoRequest> validator)
    {
        _candidatoService = candidatoService;
        _validator = validator;
    }

    [HttpPost]
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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CandidatoListItemResponse>>> Listar()
    {
        var candidatos = await _candidatoService.ListarAsync();
        return Ok(candidatos);
    }

    [HttpGet("{id:guid}")]
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
