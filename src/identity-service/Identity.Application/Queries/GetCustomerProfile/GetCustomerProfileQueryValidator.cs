using FluentValidation;

namespace Identity.Application.Queries.GetCustomerProfile;

public sealed class GetCustomerProfileQueryValidator : AbstractValidator<GetCustomerProfileQuery>
{
    public GetCustomerProfileQueryValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
    }
}
