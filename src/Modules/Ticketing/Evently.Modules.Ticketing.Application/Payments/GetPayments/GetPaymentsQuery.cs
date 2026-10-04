using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.Payments.GetPayments;

public sealed record GetPaymentsQuery(Guid CustomerId, int Page, int PageSize) : IQuery<GetPaymentsResponse>;
