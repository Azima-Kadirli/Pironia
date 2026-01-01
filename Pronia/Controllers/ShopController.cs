using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Context;
using Pronia.ViewModels.ProductViewModels;

namespace Pronia.Controllers;

public class ShopController(AppDbContext context) : Controller
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
}