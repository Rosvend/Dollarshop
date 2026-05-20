namespace Identity.Application.Dtos;

/// <summary>Open Host profile exposed to other services (e.g. sales-service).</summary>
public sealed record CustomerProfileDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    string Phone,
    bool Active);
