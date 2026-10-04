using System.Data.Common;
using Dapper;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Application.PromoCodes.GetPromoCodes;

internal sealed class GetPromoCodesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetPromoCodesQuery, GetPromoCodesResponse>
{
    public async Task<Result<GetPromoCodesResponse>> Handle(GetPromoCodesQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

        int page = request.Page < 1 ? 1 : request.Page;
        int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var parameters = new GetPromoCodesParameters(pageSize, (page - 1) * pageSize);

        IReadOnlyList<PromoCodeResponse> promoCodes = await GetPromoCodesAsync(connection, parameters, cancellationToken);

        int totalCount = await CountPromoCodesAsync(connection, cancellationToken);

        return new GetPromoCodesResponse(page, pageSize, totalCount, promoCodes);
    }

    private static async Task<IReadOnlyList<PromoCodeResponse>> GetPromoCodesAsync(
        DbConnection connection,
        GetPromoCodesParameters parameters,
        CancellationToken cancellationToken)
    {
        const string sql =
            $"""
             SELECT
                 id AS {nameof(PromoCodeResponse.Id)},
                 code AS {nameof(PromoCodeResponse.Code)},
                 discount_type AS {nameof(PromoCodeResponse.DiscountType)},
                 discount_value AS {nameof(PromoCodeResponse.DiscountValue)},
                 currency AS {nameof(PromoCodeResponse.Currency)},
                 max_redemptions AS {nameof(PromoCodeResponse.MaxRedemptions)},
                 times_redeemed AS {nameof(PromoCodeResponse.TimesRedeemed)},
                 valid_from_utc AS {nameof(PromoCodeResponse.ValidFromUtc)},
                 valid_until_utc AS {nameof(PromoCodeResponse.ValidUntilUtc)}
             FROM ticketing.promo_codes
             ORDER BY valid_from_utc DESC
             OFFSET @Skip
             LIMIT @Take
             """;

        List<PromoCodeResponse> promoCodes = (await connection.QueryAsync<PromoCodeResponse>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).AsList();

        return promoCodes;
    }

    private static async Task<int> CountPromoCodesAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT COUNT(*)
            FROM ticketing.promo_codes
            """;

        int totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return totalCount;
    }

    private sealed record GetPromoCodesParameters(int Take, int Skip);
}
