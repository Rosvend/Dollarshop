using Identity.Api.Contracts;
using Identity.Application.Commands.Authenticate;
using Identity.Application.Commands.RegisterCustomer;
using Identity.Application.Dtos;
using Identity.Application.Queries.GetCustomerProfile;

namespace Identity.Api.Mapping;

internal static class ContractMappings
{
    public static RegisterCustomerCommand ToCommand(this RegisterCustomerRequest request) =>
        new(request.Username, request.Password, request.Email, request.FullName, request.Phone);

    public static AuthenticateCommand ToCommand(this LoginRequest request) =>
        new(request.Username, request.Password);

    public static GetCustomerProfileQuery ToQuery(Guid userId) => new(userId);

    public static LoginResponse ToResponse(this LoginResultDto dto) =>
        new(dto.UserId, dto.Username, dto.FullName, dto.Email);

    public static CustomerProfileResponse ToResponse(this CustomerProfileDto dto) =>
        new(dto.Id, dto.Username, dto.FullName, dto.Email, dto.Phone, dto.Active);
}
