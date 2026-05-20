using Identity.Application.Dtos;
using Identity.Application.Exceptions;
using Identity.Domain.Interfaces;
using Identity.Domain.ValueObjects;
using MediatR;

namespace Identity.Application.Commands.Authenticate;

public sealed class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, LoginResultDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordVerifier _passwordVerifier;

    public AuthenticateCommandHandler(IUserRepository users, IPasswordVerifier passwordVerifier)
    {
        _users = users;
        _passwordVerifier = passwordVerifier;
    }

    public async Task<LoginResultDto> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByUsernameAsync(new Username(request.Username), cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (!user.Authenticate(request.Password, _passwordVerifier))
        {
            throw new InvalidCredentialsException();
        }

        return new LoginResultDto(
            user.Id.Value,
            user.Username.Value,
            user.Profile.Name.Value,
            user.Profile.Email.Value);
    }
}
