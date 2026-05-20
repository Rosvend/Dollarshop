namespace Identity.Application.Dtos;

public sealed record LoginResultDto(
    Guid UserId,
    string Username,
    string FullName,
    string Email);
