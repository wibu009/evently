using Evently.Common.Application.Caching;

namespace Evently.Modules.Ticketing.Application.Carts;

public sealed class CartService(ICacheService cacheService)
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(60);
    private static readonly TimeSpan LocalCacheExpiration = TimeSpan.FromMinutes(15);

    public async Task<Cart> GetAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        string cacheKey = CreateCacheKey(customerId);

        Cart cart = await cacheService.GetAsync<Cart>(cacheKey, cancellationToken) ?? Cart.CreateDefault(customerId);

        return cart;
    }
    
    public async Task ClearAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        string cacheKey = CreateCacheKey(customerId);

        var cart = Cart.CreateDefault(customerId);

        await cacheService.SetAsync(
            cacheKey,
            cart,
            DefaultExpiration,
            LocalCacheExpiration,
            cancellationToken: cancellationToken);
    }

    public async Task AddItemAsync(Guid customerId, CartItem cartItem, CancellationToken cancellationToken = default)
    {
        string cacheKey = CreateCacheKey(customerId);
        
        Cart cart = await GetAsync(customerId, cancellationToken);
        CartItem? existingCartItem = cart.Items.FirstOrDefault(c => c.TicketTypeId == cartItem.TicketTypeId);
        
        if (existingCartItem is null)
        {
            cart.Items.Add(cartItem);
        }
        else
        {
            existingCartItem.Quantity += cartItem.Quantity;
        }
        
        await cacheService.SetAsync(
            cacheKey,
            cart,
            DefaultExpiration,
            LocalCacheExpiration,
            cancellationToken: cancellationToken);
    }
    
    public async Task RemoveItemAsync(Guid customerId, Guid ticketTypeId, CancellationToken cancellationToken = default)
    {
        string cacheKey = CreateCacheKey(customerId);

        Cart cart = await GetAsync(customerId, cancellationToken);
        CartItem? cartItem = cart.Items.Find(c => c.TicketTypeId == ticketTypeId);

        if (cartItem is null)
        {
            return;
        }

        cart.Items.Remove(cartItem);

        await cacheService.SetAsync(
            cacheKey,
            cart,
            DefaultExpiration,
            LocalCacheExpiration,
            cancellationToken: cancellationToken);
    }
    
    private static string CreateCacheKey(Guid customerId) => $"cart:{customerId}";
}
