using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Context;
using Pronia.ViewModels.ProductViewModels;


namespace Pronia.Areas.Admin.Controllers;
[Area("Admin")]
public class ProductController : Controller
{
    private readonly AppDbContext context;
    private readonly IWebHostEnvironment _environment;
    public ProductController(AppDbContext context, IWebHostEnvironment environment)
    {
        this.context = context;
        _environment = environment;
    }

    
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
    public async Task<IActionResult> Create(ProductCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);   
        }

        // var existedCategory = await context.Categories.AnyAsync(x=>x.Id==product.CategoryId);
        // if (!existedCategory)
        // {
        //     ModelState.AddModelError("CategoryId", "Bele bir kateqoriya yoxdur");
        //     return View(product);
        // }


        
        
        
        string uniqueMainImageName = Guid.NewGuid().ToString() + vm.MainImage;
        string mainPath = Path.Combine(_environment.WebRootPath,"assets","images","website-images",uniqueMainImageName);  
        using FileStream mainStream = new (mainPath, FileMode.Create);
        await vm.MainImage.CopyToAsync(mainStream);
        
        string uniqueHoverImageName = Guid.NewGuid().ToString() + vm.MainImage;
        string hoverPath = Path.Combine(_environment.WebRootPath,"assets","images","website-images",uniqueHoverImageName);  
        using FileStream hoverStream = new (hoverPath, FileMode.Create);
        await vm.HoverImage.CopyToAsync(hoverStream);
        
        Product product = new()
        {
            Name = vm.Name,
            Description = vm.Description,
            Price = vm.Price,
            CategoryId = vm.CategoryId,
            HoverImagePath = uniqueHoverImageName,
            MainImagePath = uniqueMainImageName
        };
        if (!vm.MainImage.ContentType.Contains("image"))
        {
            ModelState.AddModelError("MainImage", "Ancaq image tipinde data elave ede bilersiniz");
            return View(product);
        }

        if (vm.MainImage.Length > 2 * 1024 * 1024)
        {
            ModelState.AddModelError("MainImage", "Seklin maksimum uzunlugu 2mb-dan cox olmamalidir");
            return View(product);
        }
        if (!vm.HoverImage.ContentType.Contains("image"))
        {
            ModelState.AddModelError("HoverImage", "Ancaq image tipinde data elave ede bilersiniz");
            return View(product);
        }

        if (vm.HoverImage.Length > 2 * 1024 * 1024)
        {
            ModelState.AddModelError("HoverImage", "Seklin maksimum uzunlugu 2mb-dan cox olmamalidir");
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
        //existedProduct.ImagePath = product.ImagePath;
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
        
        string folderPath = Path.Combine(_environment.WebRootPath,"assets","images","website-images");  
        string mainImagePath = Path.Combine(folderPath, product.MainImagePath);
        string hoverImagePath = Path.Combine(folderPath, product.HoverImagePath);
        if(System.IO.File.Exists(mainImagePath))
        System.IO.File.Delete(mainImagePath);
        if(System.IO.File.Exists(hoverImagePath))
        System.IO.File.Delete(hoverImagePath);
        
        return RedirectToAction(nameof(Index));
    }
    
    
   
}