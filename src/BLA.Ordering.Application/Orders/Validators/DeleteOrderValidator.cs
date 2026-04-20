using BLA.Ordering.Application.Orders.Commands;
using FluentValidation;

namespace BLA.Ordering.Application.Orders.Validators;

public sealed class DeleteOrderValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order id is required.")
            .MaximumLength(120).WithMessage("Order id must not exceed 120 characters.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer id is required.");
    }
}
