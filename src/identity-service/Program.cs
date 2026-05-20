var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
var app = builder.Build();

app.MapHealthChecks("/health");

// Any GUID returns the same fake profile — the demo only needs identity to
// "exist" so a customerId can be passed into POST /carts.
app.MapGet("/users/{userId:guid}", (Guid userId) => Results.Ok(new UserProfile(
    Id: userId,
    FullName: "Demo Customer",
    Email: "demo@dollarshop.test",
    Phone: "+57 300 555 1234",
    Active: true)));

app.Run();

internal sealed record UserProfile(
    Guid Id,
    string FullName,
    string Email,
    string Phone,
    bool Active);
