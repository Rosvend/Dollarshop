using MediatR;
using Sales.Application.Sagas;

namespace Sales.Application.Commands.Checkout;

/// <summary>
/// Thin entry point of the checkout use case: it delegates the whole flow to
/// the <see cref="CheckoutSagaOrchestrator"/>. The transaction is committed by
/// the pipeline's <c>TransactionBehavior</c>.
/// </summary>
public sealed class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, Unit>
{
    private readonly CheckoutSagaOrchestrator _orchestrator;

    public CheckoutCommandHandler(CheckoutSagaOrchestrator orchestrator) =>
        _orchestrator = orchestrator;

    public async Task<Unit> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        await _orchestrator.StartAsync(request.CartId, request.PaymentMethod, cancellationToken);
        return Unit.Value;
    }
}
