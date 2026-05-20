using FluentValidation;

namespace Catalog.Application.Commands.PublishProduct;

public sealed class PublishProductCommandValidator : AbstractValidator<PublishProductCommand>
{
    public PublishProductCommandValidator()
    {
        RuleFor(command => command.Sku).NotEmpty().MaximumLength(64);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.ListPrice).GreaterThanOrEqualTo(0m);
        RuleFor(command => command.Currency).NotEmpty();
        RuleFor(command => command.InitialStock).GreaterThanOrEqualTo(0);
    }
}
