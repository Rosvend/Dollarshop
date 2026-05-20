using Catalog.Api.Contracts;
using Catalog.Api.Mapping;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

/// <summary>
/// Stock reservation endpoints consumed by <c>sales-service</c> during checkout
/// (same paths as the previous minimal stub).
/// </summary>
[ApiController]
[Produces("application/json")]
public sealed class StockReservationsController : ControllerBase
{
    private readonly ISender _sender;

    public StockReservationsController(ISender sender) => _sender = sender;

    [HttpPost("stock-reservations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reserve(
        [FromBody] StockReservationRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(request.ToReserveCommand(), cancellationToken);
        return Ok();
    }

    [HttpPost("stock-reservations/release")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Release(
        [FromBody] StockReservationRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(request.ToReleaseCommand(), cancellationToken);
        return Ok();
    }
}
