using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Context;

namespace Pronia.Controllers;

public class ShopController(AppDbContext context) : Controller
{
    // GET
    public async Task<IActionResult> Index()
    {
        var products = await context.Products.ToListAsync();
        return View(products);
    }
}