namespace Identity.Api.Contracts;

public sealed record RegisterCustomerRequest(
    string Username,
    string Password,
    string Email,
    string FullName,
    string Phone);

public sealed record LoginRequest(string Username, string Password);
