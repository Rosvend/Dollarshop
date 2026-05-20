using Catalog.Api.Contracts;
using Catalog.Api.Mapping;
using Catalog.Application.Queries.GetProduct;
using Catalog.Application.Queries.ListProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("products")]
[Produces("application/json")]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(ProductListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var products = await _sender.Send(ContractMappings.ToListQuery(), cancellationToken);
        return Ok(new ProductListResponse(products.Select(product => product.ToResponse()).ToList()));
    }

    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _sender.Send(new GetProductQuery(productId), cancellationToken);
        return Ok(product.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(typeof(PublishProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Publish(
        [FromBody] PublishProductRequest request,
        CancellationToken cancellationToken)
    {
        var productId = await _sender.Send(request.ToCommand(), cancellationToken);

        return CreatedAtAction(
            nameof(Get),
            new { productId },
            new PublishProductResponse(productId));
    }
}
