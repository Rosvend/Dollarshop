using FluentValidation;

namespace Identity.Application.Commands.Authenticate;

public sealed class AuthenticateCommandValidator : AbstractValidator<AuthenticateCommand>
{
    public AuthenticateCommandValidator()
    {
        RuleFor(command => command.Username).NotEmpty();
        RuleFor(command => command.Password).NotEmpty();
    }
}
