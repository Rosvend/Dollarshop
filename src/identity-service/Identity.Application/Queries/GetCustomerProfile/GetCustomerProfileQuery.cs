using Identity.Application.Dtos;
using Identity.Application.Messaging;

namespace Identity.Application.Queries.GetCustomerProfile;

public sealed record GetCustomerProfileQuery(Guid UserId) : IQuery<CustomerProfileDto>;
