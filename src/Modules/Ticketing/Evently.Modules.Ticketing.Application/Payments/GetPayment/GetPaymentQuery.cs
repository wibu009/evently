using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.Payments.GetPayment;

public sealed record GetPaymentQuery(Guid PaymentId) : IQuery<PaymentResponse>;
