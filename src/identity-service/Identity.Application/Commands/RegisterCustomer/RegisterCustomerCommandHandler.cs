using Identity.Application.Exceptions;
using Identity.Domain.Aggregates;
using Identity.Domain.Interfaces;
using Identity.Domain.ValueObjects;
using MediatR;

namespace Identity.Application.Commands.RegisterCustomer;

public sealed class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, Guid>
{
    private readonly IUserRepository _users;
    private readonly IPasswordVerifier _passwordVerifier;

    public RegisterCustomerCommandHandler(IUserRepository users, IPasswordVerifier passwordVerifier)
    {
        _users = users;
        _passwordVerifier = passwordVerifier;
    }

    public async Task<Guid> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        var username = new Username(request.Username);
        var email = new Email(request.Email);

        if (await _users.ExistsByUsernameAsync(username, cancellationToken))
        {
            throw new DuplicateRegistrationException($"Username '{username.Value}' is already taken.");
        }

        if (await _users.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new DuplicateRegistrationException($"Email '{email.Value}' is already registered.");
        }

        var user = new User(
            UserId.New(),
            username,
            _passwordVerifier.Hash(request.Password),
            new RegistrationData(
                email,
                new PersonName(request.FullName),
                new PhoneNumber(request.Phone)));

        await _users.SaveAsync(user, cancellationToken);

        return user.Id.Value;
    }
}
