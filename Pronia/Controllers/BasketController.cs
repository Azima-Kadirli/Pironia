using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Abstraction;
using Pronia.Context;

namespace Pronia.Controllers;

public class BasketController : Controller
{
    readonly IBasketService _service;
    readonly AppDbContext _context;

    public BasketController(IBasketService service, AppDbContext context)
    {
        _service = service;
        _context = context;
    }

    public async  Task<IActionResult> Index()
    {
        var basketItems = await _service.GetBasketItemsAsync();
        return View(basketItems);
    }

    public async Task<IActionResult> DecreaseBasketItemCount(int productId)
    {
        var isExistProduct = await _context.Products.AnyAsync(x=>x.Id==productId);
        if(isExistProduct == false)
            return NotFound();
        
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var isExistUser = await _context.Users.AnyAsync(x=>x.Id==userId);

        if(isExistUser == false)
            return BadRequest();

        var basketItem =
            await _context.BasketItems.FirstOrDefaultAsync(x => x.AppUserId == userId && x.ProductId == productId);
        if (basketItem is null)
            return NotFound();

        if (basketItem.Count > 1)
            basketItem.Count--;
        _context.BasketItems.Update(basketItem);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}