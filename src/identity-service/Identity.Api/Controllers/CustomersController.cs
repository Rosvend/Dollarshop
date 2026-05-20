using Identity.Api.Contracts;
using Identity.Api.Mapping;
using Identity.Application.Queries.GetCustomerProfile;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("customers")]
[Produces("application/json")]
public sealed class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(RegisterCustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = await _sender.Send(request.ToCommand(), cancellationToken);

        return CreatedAtAction(
            nameof(GetProfile),
            new { customerId },
            new RegisterCustomerResponse(customerId));
    }

    [HttpGet("{customerId:guid}")]
    [ProducesResponseType(typeof(CustomerProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(Guid customerId, CancellationToken cancellationToken)
    {
        var profile = await _sender.Send(new GetCustomerProfileQuery(customerId), cancellationToken);
        return Ok(profile.ToResponse());
    }
}
