using Identity.Application.Dtos;
using Identity.Application.Exceptions;
using Identity.Domain.Interfaces;
using Identity.Domain.ValueObjects;
using MediatR;

namespace Identity.Application.Queries.GetCustomerProfile;

public sealed class GetCustomerProfileQueryHandler
    : IRequestHandler<GetCustomerProfileQuery, CustomerProfileDto>
{
    private readonly IUserRepository _users;

    public GetCustomerProfileQueryHandler(IUserRepository users) => _users = users;

    public async Task<CustomerProfileDto> Handle(
        GetCustomerProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(new UserId(request.UserId), cancellationToken)
            ?? throw new UserNotFoundException(request.UserId);

        return new CustomerProfileDto(
            user.Id.Value,
            user.Username.Value,
            user.Profile.Name.Value,
            user.Profile.Email.Value,
            user.Profile.Phone.Value,
            user.Active);
    }
}
