using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sales.Api.Contracts;
using Sales.Api.Mapping;
using Sales.Application.Queries.GetCart;

namespace Sales.Api.Controllers;

/// <summary>
/// Thin HTTP entry point for the Sales use cases (rubric: "Controllers delgados").
/// Every action does exactly three things: map the HTTP request to an Application
/// Command/Query, dispatch it through MediatR, and translate the outcome to a
/// status code. It holds no business logic and manages no transactions — the
/// Application pipeline (<c>ValidationBehavior</c>, <c>TransactionBehavior</c>)
/// owns those concerns.
/// </summary>
[ApiController]
[Route("carts")]
[Produces("application/json")]
public sealed class CartsController : ControllerBase
{
    private readonly ISender _sender;

    public CartsController(ISender sender) => _sender = sender;

    /// <summary>Opens a new, empty shopping cart for a customer.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateCartResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCart(
        [FromBody] CreateCartRequest request,
        CancellationToken cancellationToken)
    {
        var cartId = await _sender.Send(request.ToCommand(), cancellationToken);

        return CreatedAtAction(
            nameof(GetCart),
            new { cartId },
            new CreateCartResponse(cartId));
    }

    /// <summary>Returns the current state of a cart.</summary>
    [HttpGet("{cartId:guid}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCart(Guid cartId, CancellationToken cancellationToken)
    {
        var cart = await _sender.Send(new GetCartQuery(cartId), cancellationToken);
        return Ok(cart.ToResponse());
    }

    /// <summary>Adds a product to a cart (consolidating the quantity if already present).</summary>
    [HttpPost("{cartId:guid}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(
        Guid cartId,
        [FromBody] AddItemRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(request.ToCommand(cartId), cancellationToken);
        return Ok();
    }

    /// <summary>Removes a product from a cart.</summary>
    [HttpDelete("{cartId:guid}/items/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(
        Guid cartId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(ContractMappings.ToRemoveItemCommand(cartId, productId), cancellationToken);
        return NoContent();
    }

    /// <summary>Applies a discount policy to a cart.</summary>
    [HttpPost("{cartId:guid}/discounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ApplyDiscount(
        Guid cartId,
        [FromBody] ApplyDiscountRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(request.ToCommand(cartId), cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Starts the checkout Saga for a cart. Returns <c>202 Accepted</c>: stock is
    /// reserved and payment is requested synchronously, but the payment outcome
    /// arrives asynchronously as an integration event (Microservices §3.3).
    /// </summary>
    [HttpPost("{cartId:guid}/checkout")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Checkout(
        Guid cartId,
        [FromBody] CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(request.ToCommand(cartId), cancellationToken);
        return Accepted();
    }
}
