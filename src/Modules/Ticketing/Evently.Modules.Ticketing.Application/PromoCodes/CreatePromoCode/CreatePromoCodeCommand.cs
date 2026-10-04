using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.PromoCodes.CreatePromoCode;

public sealed record CreatePromoCodeCommand(
    string Code,
    int DiscountType,
    decimal DiscountValue,
    string Currency,
    int? MaxRedemptions,
    DateTime? ValidFromUtc,
    DateTime? ValidUntilUtc) : ICommand<Guid>;
