using Sales.Api.Errors;
using Sales.Application;
using Sales.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Structured logging to the console (Microservices §7.3 — observability).
builder.Services.AddSerilog((services, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// External layer: controllers + OpenAPI/Swagger.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Boundary error handling — every exception leaves as RFC 7807 ProblemDetails.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHealthChecks();

// Inner layers — Phase 2 (Application) and Phase 3 (Infrastructure) composition roots.
builder.Services.AddSalesApplication();
builder.Services.AddSalesInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
