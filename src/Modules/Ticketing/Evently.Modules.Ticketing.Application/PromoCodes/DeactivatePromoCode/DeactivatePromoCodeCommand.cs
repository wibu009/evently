using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.PromoCodes.DeactivatePromoCode;

public sealed record DeactivatePromoCodeCommand(Guid PromoCodeId) : ICommand;
