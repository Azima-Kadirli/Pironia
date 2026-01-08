using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Pronia.Abstraction;
using Pronia.Context;

namespace Pronia.Services;

public class BasketService(AppDbContext _context,IHttpContextAccessor _accessor):IBasketService
{
    public async Task<List<BasketItem>> GetBasketItemsAsync()
    {
       string userId =   _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
       
       var isExistUser = await _context.Users.AnyAsync(x=>x.Id == userId);
       if (!isExistUser)
           return [];
       
       var basketItems = await _context.BasketItems.Include(x=>x.Product).Where(x=>x.AppUserId == userId).ToListAsync();
       return basketItems;
    }
    
}