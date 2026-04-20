using BLA.Ordering.Application.Orders.Commands;
using FluentValidation;

namespace BLA.Ordering.Application.Orders.Validators;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    private static readonly string[] AllowedCurrencies = ["USD", "EUR"];

    public CreateOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order id is required.")
            .MaximumLength(120).WithMessage("Order id must not exceed 120 characters.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer id is required.");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required.")
            .MaximumLength(300).WithMessage("Shipping address must not exceed 300 characters.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Must(currency => AllowedCurrencies.Contains(currency.Trim().ToUpperInvariant()))
            .WithMessage("Currency must be either USD or EUR.");
    }
}
