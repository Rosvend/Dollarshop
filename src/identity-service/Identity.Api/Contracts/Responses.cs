namespace Identity.Api.Contracts;

public sealed record RegisterCustomerResponse(Guid CustomerId);

public sealed record LoginResponse(
    Guid UserId,
    string Username,
    string FullName,
    string Email);

public sealed record CustomerProfileResponse(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    string Phone,
    bool Active);
