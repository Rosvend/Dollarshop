using Identity.Application.Dtos;
using Identity.Application.Messaging;

namespace Identity.Application.Commands.Authenticate;

public sealed record AuthenticateCommand(string Username, string Password) : ICommand<LoginResultDto>;
