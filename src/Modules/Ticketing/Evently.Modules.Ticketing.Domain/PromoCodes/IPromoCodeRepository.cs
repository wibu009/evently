namespace Evently.Modules.Ticketing.Domain.PromoCodes;

public interface IPromoCodeRepository
{
    Task<PromoCode?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PromoCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    void Insert(PromoCode promoCode);
}
