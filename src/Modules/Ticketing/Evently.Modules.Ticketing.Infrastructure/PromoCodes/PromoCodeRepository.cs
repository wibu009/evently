using Evently.Modules.Ticketing.Domain.PromoCodes;
using Evently.Modules.Ticketing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Ticketing.Infrastructure.PromoCodes;

internal sealed class PromoCodeRepository(TicketingDbContext context) : IPromoCodeRepository
{
    public async Task<PromoCode?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.PromoCodes.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PromoCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        string normalizedCode = code.Trim().ToUpperInvariant();

        return await context.PromoCodes.SingleOrDefaultAsync(
            p => p.Code == normalizedCode,
            cancellationToken);
    }

    public void Insert(PromoCode promoCode)
    {
        context.PromoCodes.Add(promoCode);
    }
}
