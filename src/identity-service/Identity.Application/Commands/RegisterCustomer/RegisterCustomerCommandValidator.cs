using FluentValidation;

namespace Identity.Application.Commands.RegisterCustomer;

public sealed class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(command => command.Username).NotEmpty().MaximumLength(32);
        RuleFor(command => command.Password).NotEmpty().MinimumLength(6);
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Phone).NotEmpty().MaximumLength(32);
    }
}
