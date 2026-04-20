using BLA.Ordering.Application.Orders.Commands;
using FluentValidation;

namespace BLA.Ordering.Application.Orders.Validators;

public sealed class UpdateOrderValidator : AbstractValidator<UpdateOrderCommand>
{
    private static readonly string[] AllowedCurrencies = ["USD", "EUR"];

    public UpdateOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order id is required.")
            .MaximumLength(120).WithMessage("Order id must not exceed 120 characters.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer id is required.");

        RuleFor(x => x)
            .Must(HasAtLeastOneChange)
            .WithMessage("At least one field must be provided for update.");

        RuleFor(x => x.ShippingAddress)
            .MaximumLength(300).WithMessage("Shipping address must not exceed 300 characters.")
            .When(x => x.ShippingAddress is not null);

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than zero.")
            .When(x => x.TotalAmount.HasValue);

        RuleFor(x => x.Currency)
            .Must(currency => AllowedCurrencies.Contains(currency!.Trim().ToUpperInvariant()))
            .WithMessage("Currency must be either USD or EUR.")
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));
    }

    private static bool HasAtLeastOneChange(UpdateOrderCommand command) =>
        command.ShippingAddress is not null || command.TotalAmount.HasValue || command.Currency is not null;
}
