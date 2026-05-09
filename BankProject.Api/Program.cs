using BankProject.Api.Data;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração do Serilog (Console + Arquivo)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Configuração do Banco de Dados (Oracle)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("Oracle")));

// 3. Configuração de Health Checks (Requisito Especial para Testes)
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// EXIGÊNCIA DO PROFESSOR OBRIGATÓRIA PARA OS TESTES FUNCIONAREM:
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>("oracle_db");
}

// 4. Configuração de Observabilidade (OpenTelemetry - Tracing Exporter Console)
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter();
    });


builder.Services.AddScoped<BankProject.Api.RabbitMQ.IRabbitMqProducer, BankProject.Api.RabbitMQ.RabbitMqProducer>();


builder.Services.AddHostedService<BankProject.Api.RabbitMQ.ContratacaoConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura o pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Necessário para os Testes de Integração com WebApplicationFactory
public partial class Program { }
