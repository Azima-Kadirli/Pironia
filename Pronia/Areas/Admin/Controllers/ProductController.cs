using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Context;
using Pronia.Helpers;
using Pronia.ViewModels.ProductViewModels;


namespace Pronia.Areas.Admin.Controllers;
[Area("Admin")]
public class ProductController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    public ProductController(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }
    
    public async Task<IActionResult> Index()
    {
        List<ProductGetViewModel> vm = await _context.Products.Include(x=>x.Category)
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

        var existedCategory = await _context.Categories.AnyAsync(x => x.Id == vm.CategoryId);
        if (!existedCategory)
        {
            await SendItemsWithViewBag<Category>();
            ModelState.AddModelError("CategoryId", "Bele bir kateqoriya yoxdur");
            return View(vm);
        }
        foreach (var tagId in  vm.TagIds)
        {
            var isExistTag= await  _context.Tags.AnyAsync(t => t.Id == tagId);
            if (!isExistTag)
            {
                await SendItemsWithViewBag<Tag>();
                ModelState.AddModelError("TagIds","Bele bir tag movcud deyil");
                return View(vm);
            }
        }

        string folderPath = Path.Combine(_environment.WebRootPath, "assets", "images", "website-images");
        // string uniqueMainImageName = Guid.NewGuid().ToString() + vm.MainImage.FileName;
        // string mainPath = Path.Combine(_environment.WebRootPath,"assets","images","website-images",uniqueMainImageName);  
        // using FileStream mainStream = new (mainPath, FileMode.Create);
        // await vm.MainImage.CopyToAsync(mainStream);
        //
        // string uniqueHoverImageName = Guid.NewGuid().ToString() + vm.MainImage.FileName;
        // string hoverPath = Path.Combine(_environment.WebRootPath,"assets","images","website-images",uniqueHoverImageName);  
        // using FileStream hoverStream = new (hoverPath, FileMode.Create);
       // await vm.HoverImage.CopyToAsync(hoverStream);
       string uniqueHoverImageName = await vm.HoverImage.SaveFileAsync(folderPath);
       string uniqueMainImageName = await vm.MainImage.SaveFileAsync(folderPath);
        
        Product product = new()
        {
            Name = vm.Name,
            Description = vm.Description,
            Price = vm.Price,
            CategoryId = vm.CategoryId,
            HoverImagePath = uniqueHoverImageName,
            MainImagePath = uniqueMainImageName,
            ProductTags = [],
            ProductImages = []
        };

        foreach (var image in vm.Images)
        {
            string uniqueFilePath = await image.SaveFileAsync(folderPath);
            ProductImage productImage = new()
            {
                ImagePath = uniqueFilePath,
                Product = product
            };
            product.ProductImages.Add(productImage);
        }
        
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

        foreach (var image in vm.Images)
        {
            if (!image.CheckType())
            {
                ModelState.AddModelError("Images", "Ancaq image tipinde data elave ede bilersiniz");
                return View(product);
            }

            if (!image.CheckSize(2))
            {
                ModelState.AddModelError("Images", "Seklin maksimum uzunlugu 2mb-dan cox olmamalidir");
                return View(product);
            }
        }
        
        await _context.Products.AddAsync(product);  
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
    
    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var product = await _context.Products.Include(x=>x.ProductTags).Include(x=>x.ProductImages).FirstOrDefaultAsync(x => x.Id == id);
      
        if (product == null)
        {
            return NotFound();
        }
        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.Tags = await _context.Tags.ToListAsync();
        ProductUpdateViewModel vm = new ProductUpdateViewModel()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,     
            CategoryId = product.CategoryId,
            HoverImagePath = product.HoverImagePath,
            MainImagePath = product.MainImagePath,
            TagIds = product.ProductTags.Select(t => t.TagId).ToList(),
            AdditionalImagesPath = product.ProductImages.Select(x => x.ImagePath).ToList(),
            AdditionalImagesIds = product.ProductImages.Select(x => x.Id).ToList()
        };

        return View(vm);
    }
    
    [HttpPost]
    public async Task<IActionResult> Update(ProductUpdateViewModel vm)
    {
       if (!ModelState.IsValid)
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.Tags = await _context.Tags.ToListAsync();
        return View(vm);
    }

    foreach (var tagId in vm.TagIds ?? new List<int>())
    {
        var isExistTag = await _context.Tags.AnyAsync(t => t.Id == tagId);
        if (!isExistTag)
        {
            await SendItemsWithViewBag<Tag>();
            ModelState.AddModelError("TagIds", "Bele bir tag movcud deyil");
            return View(vm);
        }
    }

    var existedProduct = await _context.Products
        .Include(x => x.ProductTags)
        .Include(x => x.ProductImages)
        .FirstOrDefaultAsync(x => x.Id == vm.Id);

    if (existedProduct == null) return BadRequest();

    if (vm.MainImage != null)
    {
        if (!vm.MainImage.CheckType())
        {
            ModelState.AddModelError("MainImage", "Ancaq image tipinde data elave ede bilersiniz");
            return View(vm);
        }
        if (vm.MainImage.CheckSize(2))
        {
            ModelState.AddModelError("MainImage", "Seklin maksimum uzunlugu 2mb-dan cox olmamalidir");
            return View(vm);
        }
    }

    if (vm.HoverImage != null)
    {
        if (!vm.HoverImage.CheckType())
        {
            ModelState.AddModelError("HoverImage", "Ancaq image tipinde data elave ede bilersiniz");
            return View(vm);
        }
        if (vm.HoverImage.CheckSize(2))
        {
            ModelState.AddModelError("HoverImage", "Seklin maksimum uzunlugu 2mb-dan cox olmamalidir");
            return View(vm);
        }
    }

    foreach (var img in vm.Images ?? new List<IFormFile>())
    {
        if (!img.CheckType())
        {
            ModelState.AddModelError("Images", "Ancaq image tipinde data elave ede bilersiniz");
            return View(vm);
        }
        if (img.CheckSize(2))
        {
            ModelState.AddModelError("Images", "Seklin maksimum uzunlugu 2mb-dan cox olmamalidir");
            return View(vm);
        }
    }

    string folderPath = Path.Combine(_environment.WebRootPath, "assets", "images", "website-images");

    if (vm.MainImage != null)
    {
        string newMainImagePath = await vm.MainImage.SaveFileAsync(folderPath);
        ExtensionMethods.DeleteFile(Path.Combine(folderPath, existedProduct.MainImagePath));
        existedProduct.MainImagePath = newMainImagePath;
    }

    if (vm.HoverImage != null)
    {
        string newHoverImagePath = await vm.HoverImage.SaveFileAsync(folderPath);
        ExtensionMethods.DeleteFile(Path.Combine(folderPath, existedProduct.HoverImagePath));
        existedProduct.HoverImagePath = newHoverImagePath;
    }

    existedProduct.CategoryId = vm.CategoryId;
    existedProduct.Name = vm.Name;
    existedProduct.Description = vm.Description;
    existedProduct.Price = vm.Price;

    existedProduct.ProductTags.Clear();
    foreach (var tagId in vm.TagIds ?? new List<int>())
    {
        existedProduct.ProductTags.Add(new ProductTag
        {
            TagId = tagId,
            ProductId = existedProduct.Id
        });
    }

    var existImages = existedProduct.ProductImages.ToList();
    foreach (var image in existImages)
    {
        bool keep = vm.AdditionalImagesIds?.Contains(image.Id) ?? false;
        if (!keep)
        {
            ExtensionMethods.DeleteFile(Path.Combine(folderPath, image.ImagePath));
            existedProduct.ProductImages.Remove(image);
        }
    }

    foreach (var image in vm.Images ?? new List<IFormFile>())
    {
        string uniqueFilePath = await image.SaveFileAsync(folderPath);
        existedProduct.ProductImages.Add(new ProductImage
        {
            ImagePath = uniqueFilePath,
            ProductId = existedProduct.Id
        });
    }

    await _context.SaveChangesAsync();
    return RedirectToAction("Index");
    }
    
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.Include(x=>x.ProductImages).FirstOrDefaultAsync(x=>x.Id == id);
        if (product is null)
            return NotFound();

        
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        
        string folderPath = Path.Combine(_environment.WebRootPath,"assets","images","website-images");  
        string mainImagePath = Path.Combine(folderPath, product.MainImagePath);
        string hoverImagePath = Path.Combine(folderPath, product.HoverImagePath);
        
        ExtensionMethods.DeleteFile(mainImagePath);
        ExtensionMethods.DeleteFile(hoverImagePath);

        foreach (var productImage in product.ProductImages)
        {
            string imagePath =  Path.Combine(folderPath, productImage.ImagePath);
            ExtensionMethods.DeleteFile(imagePath);
        }
        
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(int id)
    {
        var product = await _context.Products.Include(x=>x.Category)
            .Select(product=>new ProductGetViewModel()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                HoverImagePath =  product.HoverImagePath,
                MainImagePath =  product.MainImagePath, 
                CategoryName =  product.Category.Name,
                TagNames = product.ProductTags.Select(x=>x.Tag.Name).ToList(),
                AdditionalImagePaths = product.ProductImages.Select(x=>x.ImagePath).ToList()
            }).FirstOrDefaultAsync(x => x.Id == id);
        
        if (product is null)
            return NotFound();
        return View(product);
    }

    private async Task SendItemsWithViewBag<T>() where T : class
    {
        var categories = await _context.Set<T>().ToListAsync();
        ViewBag.Categories = categories;
        var tags = await _context.Tags.ToListAsync();
        ViewBag.Tags = tags;
    }
}