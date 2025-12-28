using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Context;
using Pronia.Helpers;
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
    
    public async Task<IActionResult> Index()
    {
        List<ProductGetViewModel> vm = await context.Products.Include(x=>x.Category)
            .Select(product=>new ProductGetViewModel()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                HoverImagePath = product.HoverImagePath,
                MainImagePath = product.MainImagePath,
                CategoryName = product.Category.Name
            }).ToListAsync();
        return View(vm);
        
        
    }

    public async Task<IActionResult> Create()
    {
        await SendItemsWithViewBag<Category>();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);   
        }

        var existedCategory = await context.Categories.AnyAsync(x => x.Id == vm.CategoryId);
        if (!existedCategory)
        {
            await SendItemsWithViewBag<Category>();
            ModelState.AddModelError("CategoryId", "Bele bir kateqoriya yoxdur");
            return View(vm);
        }
        foreach (var tagId in  vm.TagIds)
        {
            var isExistTag= await  context.Tags.AnyAsync(t => t.Id == tagId);
            if (!isExistTag)
            {
                await SendItemsWithViewBag<Tag>();
                ModelState.AddModelError("TagIds","Bele bir tag movcud deyil");
                return View(vm);
            }
        }
        string uniqueMainImageName = Guid.NewGuid().ToString() + vm.MainImage.FileName;
        string mainPath = Path.Combine(_environment.WebRootPath,"assets","images","website-images",uniqueMainImageName);  
        using FileStream mainStream = new (mainPath, FileMode.Create);
        await vm.MainImage.CopyToAsync(mainStream);
        
        string uniqueHoverImageName = Guid.NewGuid().ToString() + vm.MainImage.FileName;
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
            MainImagePath = uniqueMainImageName,
            ProductTags = []
        };

        foreach (var tagId in vm.TagIds)
        {
            ProductTag productTag = new()
            {
                TagId =  tagId,
                Product = product
            };
            product.ProductTags.Add(productTag);
        }
        if (!vm.MainImage.CheckType())
        {
            ModelState.AddModelError("MainImage", "Ancaq image tipinde data elave ede bilersiniz");
            return View(product);
        }

        if (!vm.MainImage.CheckSize(2))
        {
            ModelState.AddModelError("MainImage", "Seklin maksimum uzunlugu 2mb-dan cox olmamalidir");
            return View(product);
        }
        if (!vm.HoverImage.CheckType())
        {
            ModelState.AddModelError("HoverImage", "Ancaq image tipinde data elave ede bilersiniz");
            return View(product);
        }

        if (!vm.HoverImage.CheckSize(2))
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
        var product = await context.Products.Include(x=>x.ProductTags).FirstOrDefaultAsync(x => x.Id == id);
      
        if (product == null)
        {
            return NotFound();
        }
        ViewBag.Categories = await context.Categories.ToListAsync();
        ViewBag.Tags = await context.Tags.ToListAsync();
        ProductUpdateViewModel vm = new ProductUpdateViewModel()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,     
            CategoryId = product.CategoryId,
            HoverImagePath = product.HoverImagePath,
            MainImagePath = product.MainImagePath,
            TagIds = product.ProductTags.Select(t => t.TagId).ToList()
        };

        return View(vm);
    }
    
    [HttpPost]
    public async Task<IActionResult> Update(ProductUpdateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var categories = await context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            return View(vm);
        }
        foreach (var tagId in  vm.TagIds)
        {
            var isExistTag= await  context.Tags.AnyAsync(t => t.Id == tagId);
            if (!isExistTag)
            {
                await SendItemsWithViewBag<Tag>();
                ModelState.AddModelError("TagIds","Bele bir tag movcud deyil");
                return View(vm);
            }
        }
        
        var existedProduct = await context.Products.Include(x=>x.ProductTags).FirstOrDefaultAsync(x => x.Id == vm.Id);
        if (existedProduct == null)
        {
            return BadRequest();
        }
        
        if (!vm.MainImage?.CheckType() ?? false)
        {
            ModelState.AddModelError("MainImage", "Ancaq image tipinde data elave ede bilersiniz");
            return View(vm);
        }

        if (vm.MainImage?.CheckSize(2) ?? false)
        {
            ModelState.AddModelError("MainImage", "Seklin maksimum uzunlugu 2mb-dan cox olmamalidir");
            return View(vm);
        }
        if (!vm.HoverImage?.CheckType()  ?? false)
        {
            ModelState.AddModelError("HoverImage", "Ancaq image tipinde data elave ede bilersiniz");
            return View(vm);
        }

        if (vm.HoverImage?.CheckSize(2) ??  false)
        {
            ModelState.AddModelError("HoverImage", "Seklin maksimum uzunlugu 2mb-dan cox olmamalidir");
            return View(vm);
        }
        
        string folderPath = Path.Combine(_environment.WebRootPath,"assets","images","website-images");
        if (vm.MainImage is { })
        {
            string newMainImagePath = await vm.MainImage.SaveFileAsync(folderPath);
            
            string existMainImagePath = Path.Combine(folderPath, existedProduct.MainImagePath);
            ExtensionMethods.DeleteFile(existMainImagePath);
            existedProduct.MainImagePath = newMainImagePath;
        }
        
        if (vm.HoverImage is { })
        {
            string newHoverImagePath = await vm.HoverImage.SaveFileAsync(folderPath);
            
            string existHoverImagePath = Path.Combine(folderPath, existedProduct.HoverImagePath);
            ExtensionMethods.DeleteFile(existHoverImagePath);
            existedProduct.HoverImagePath = newHoverImagePath;
        }
        
        existedProduct.CategoryId = vm.CategoryId;
        existedProduct.Name = vm.Name;
        existedProduct.Description = vm.Description;
        existedProduct.Price = vm.Price;
        existedProduct.ProductTags = [];
        //existedProduct.ImagePath = product.ImagePath;

        foreach (var tagId in vm.TagIds)
        {
            ProductTag productTag = new()
            {
                TagId =  tagId,
                ProductId = existedProduct.Id
            };
            existedProduct.ProductTags.Add(productTag);
        }
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

    public async Task<IActionResult> Detail(int id)
    {
        var product = await context.Products.Include(x=>x.Category)
            .Select(product=>new ProductGetViewModel()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                HoverImagePath =  product.HoverImagePath,
                MainImagePath =  product.MainImagePath, 
                CategoryName =  product.Category.Name,
                TagNames = product.ProductTags.Select(x=>x.Tag.Name).ToList()
            }).FirstOrDefaultAsync(x => x.Id == id);
        
        if (product is null)
            return NotFound();
        return View(product);
    }

    private async Task SendItemsWithViewBag<T>() where T : class
    {
        var categories = await context.Set<T>().ToListAsync();
        ViewBag.Categories = categories;
        var tags = await context.Tags.ToListAsync();
        ViewBag.Tags = tags;
    }
}