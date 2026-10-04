using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Domain.PromoCodes;

public sealed class PromoCodeRedeemedDomainEvent(Guid promoCodeId, string code, int timesRedeemed) : DomainEvent
{
    public Guid PromoCodeId { get; init; } = promoCodeId;

    public string Code { get; init; } = code;

    public int TimesRedeemed { get; init; } = timesRedeemed;
}
