using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.PromoCodes.RedeemPromoCode;

public sealed record RedeemPromoCodeCommand(Guid OrderId) : ICommand;
