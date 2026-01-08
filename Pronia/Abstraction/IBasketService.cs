namespace Pronia.Abstraction;

public interface IBasketService
{
    Task<List<BasketItem>> GetBasketItemsAsync();
}