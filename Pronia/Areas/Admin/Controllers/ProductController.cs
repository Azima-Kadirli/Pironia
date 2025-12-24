using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Context;


namespace Pronia.Areas.Admin.Controllers;
[Area("Admin")]
public class ProductController(AppDbContext context) : Controller
{
    // GET
    public async Task<IActionResult> Index()
    {
        var products = await context.Products.Include(x=>x.Category).ToListAsync();
        return View(products);
    }


    public async Task<IActionResult> Create()
    {
        var categories = await context.Categories.ToListAsync();
        ViewBag.Categories = categories;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            var categories = await context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            return View(product);   
        }
        var existedCategory = await context.Categories.AnyAsync(x=>x.Id==product.CategoryId);
        if (!existedCategory)
        {
            ModelState.AddModelError("CategoryId", "Bele bir kateqoriya yoxdur");
            return View(product);
        }
        await context.Products.AddAsync(product);  
        await context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

   

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var categories = await context.Categories.ToListAsync();
        ViewBag.Categories = categories;
        var products = await context.Products.FindAsync(id);
        if (products == null)
        {
            return NotFound();
        }

        return View(products);
    }


    [HttpPost]
    public async Task<IActionResult> Update(Product product)
    {
        if (!ModelState.IsValid)
        {
            var categories = await context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            return View(product);
        }
        var existedProduct = await context.Products.FindAsync(product.Id);
        if (existedProduct == null)
        {
            return BadRequest();
        }
        existedProduct.CategoryId = product.CategoryId;
        existedProduct.Name = product.Name;
        existedProduct.Description = product.Description;
        existedProduct.Price = product.Price;
        existedProduct.ImagePath = product.ImagePath;
        context.Update(existedProduct);
        await context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
    
    public async Task<IActionResult> Delete(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product is null)
            return NotFound();

        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
   
}