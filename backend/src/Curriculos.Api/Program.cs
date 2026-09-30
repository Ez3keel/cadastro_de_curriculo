using Curriculos.Api.Data;
using Curriculos.Api.Middleware;
using Curriculos.Api.Services;
using Curriculos.Api.Services.Pdf;
using Curriculos.Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = PdfFileValidator.TamanhoMaximoEmBytes);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
