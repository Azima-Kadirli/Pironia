using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Abstraction;
using Pronia.Context;
using Pronia.ViewModels.ProductViewModels;

namespace Pronia.Controllers;

public class ShopController(AppDbContext context,IEmailService _service) : Controller
{
    public async Task<IActionResult> Index()
    {
        var products = await context.Products.ToListAsync();
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var product = await context.Products.Select(x => new ProductGetViewModel()
        {
            Id =  x.Id,
            Name = x.Name,
            Price = x.Price,
            Description = x.Description,
            CategoryName =  x.Category.Name,
            HoverImagePath =  x.HoverImagePath,
            MainImagePath =   x.MainImagePath,
            TagNames = x.ProductTags.Select(x=>x.Tag.Name).ToList(),
            AdditionalImagePaths = x.ProductImages.Select(x=>x.ImagePath).ToList()
        }).FirstOrDefaultAsync(x => x.Id == id);
        if(product is null)
            return NotFound();
        return View(product);
    }

    public async Task<IActionResult> Test()
    {
        await _service.SendEmailAsync("azimaqadirli@gmail.com", "MPA101", "Email service is created");
        return Ok("Ok");
    }

    [Authorize]
    public async Task<IActionResult> AddToBasket(int productId)
    {
        var isExistProduct = await context.Products.AnyAsync(x=>x.Id==productId);
        if(isExistProduct == false)
            return NotFound();
        
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var isExistUser = await context.Users.AnyAsync(x=>x.Id==userId);

        if(isExistUser == false)
            return BadRequest();
        
        
        var isExistBasketItem = await context.BasketItems.FirstOrDefaultAsync(x=>x.ProductId==productId && x.AppUserId == userId);
        if (isExistBasketItem is  { })
        {
            isExistBasketItem.Count++;
            context.BasketItems.Update(isExistBasketItem);
          
        }
        else
        {
        BasketItem basketItem = new()
        {
            ProductId =  productId,
            AppUserId = userId,
            Count = 1
        };
        await context.BasketItems.AddAsync(basketItem);
            
        }
            await context.SaveChangesAsync();
        return RedirectToAction("Index","Shop");
    }

    public async  Task<IActionResult> RemoveFromBasket(int productId)
    {
        var isExistProduct = await context.Products.AnyAsync(x=>x.Id==productId);
        if(isExistProduct == false)
            return NotFound();
        
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var isExistUser = await context.Users.AnyAsync(x=>x.Id==userId);

        if(isExistUser == false)
            return BadRequest();

        var basketItem =
            await context.BasketItems.FirstOrDefaultAsync(x => x.AppUserId == userId && x.ProductId == productId);
        if (basketItem is null)
            return NotFound();

        context.BasketItems.Remove(basketItem);
        await context.SaveChangesAsync();
        
        string? returnUrl = Request.Headers["Referer"];
        if(string.IsNullOrWhiteSpace(returnUrl))
            return Redirect(returnUrl);
        
        return RedirectToAction("Index");
    }
}