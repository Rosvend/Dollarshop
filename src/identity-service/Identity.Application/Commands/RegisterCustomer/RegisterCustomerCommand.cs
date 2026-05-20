using Identity.Application.Messaging;

namespace Identity.Application.Commands.RegisterCustomer;

public sealed record RegisterCustomerCommand(
    string Username,
    string Password,
    string Email,
    string FullName,
    string Phone) : ICommand<Guid>;
