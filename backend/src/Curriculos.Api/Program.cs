using Curriculos.Api.Data;
using Curriculos.Api.Data.Seed;
using Curriculos.Api.Middleware;
using Curriculos.Api.Services;
using Curriculos.Api.Services.Pdf;
using Curriculos.Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = PdfFileValidator.TamanhoMaximoEmBytes);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Cadastro de Currículos",
        Version = "v1",
        Description = """
            API para cadastrar e consultar candidatos, com leitura opcional de currículos em PDF.

            **Fluxo típico:**
            1. (Opcional) `POST /api/curriculos/extrair` envia um PDF e devolve nome, e-mail e telefone encontrados, sem salvar.
            2. `POST /api/candidatos` cadastra o candidato (com os dados extraídos, revisados, ou preenchidos manualmente).
            3. `GET /api/candidatos` lista os candidatos e `GET /api/candidatos/{id}` mostra os detalhes de um deles.

            Em todas as rotas, clique em **Try it out**, preencha os dados e clique em **Execute**.
            """,
    });

    var arquivoXml = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(arquivoXml))
    {
        options.IncludeXmlComments(arquivoXml);
    }
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICandidatoService, CandidatoService>();
builder.Services.AddValidatorsFromAssemblyContaining<CandidatoRequestValidator>();

builder.Services.AddScoped<ICurriculoTextExtractor, PdfPigTextExtractor>();
builder.Services.AddScoped<CurriculoParser>();

builder.Services.Configure<FormOptions>(options =>
    options.MultipartBodyLengthLimit = PdfFileValidator.TamanhoMaximoEmBytes);

var origemPermitida = builder.Configuration["Cors:AllowedOrigin"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (!string.IsNullOrWhiteSpace(origemPermitida))
        {
            policy.WithOrigins(origemPermitida).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Configuration.GetValue<bool>("Database:SeedData"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await CandidatosSeed.ExecutarAsync(db);
}

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
