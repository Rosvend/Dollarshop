using Identity.Api.Mapping;
using Identity.Application.Queries.GetCustomerProfile;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

/// <summary>
/// Legacy Open Host route kept for callers that still use <c>GET /users/{id}</c>
/// from the original demo stub.
/// </summary>
[ApiController]
[Route("users")]
[Produces("application/json")]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender) => _sender = sender;

    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(Contracts.CustomerProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _sender.Send(new GetCustomerProfileQuery(userId), cancellationToken);
        return Ok(profile.ToResponse());
    }
}
