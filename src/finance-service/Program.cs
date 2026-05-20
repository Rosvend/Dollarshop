using Finance.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddHostedService<PaymentProcessor>();
var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();
