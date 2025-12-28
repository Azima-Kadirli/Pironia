using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.Context;
using Pronia.ViewModels.CategoryViewModels;

namespace Pronia.Areas.Admin.Controllers;
[Area("Admin")]
public class CategoryController : Controller
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        List<CategoryGetViewModel> vm = await _context.Categories
            .Select(product=>new CategoryGetViewModel()
            {
                Id = product.Id,
                Name = product.Name
            }).ToListAsync();
        return View(vm);
    }
    
    [HttpGet]
     public async Task<IActionResult> Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);   
        }

        var existedCategory = await _context.Categories.ToListAsync();
        
        if (existedCategory is not {})
        {
            ModelState.AddModelError("CategoryId", "Bele bir kateqoriya yoxdur");
            return View(vm);
        }

        Category category = new Category()
        {
            Name =  vm.Name
        };
        
        await _context.Categories.AddAsync(category);  
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
     [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var category = await _context.Categories.FindAsync(id);
      
        if (category == null)
        {
            return NotFound();
        }
        CategoryUpdateViewModel vm = new CategoryUpdateViewModel()
        {
            Id = category.Id,
            Name = category.Name
        };
        return View(vm);
    }
    
    [HttpPost]
    public async Task<IActionResult> Update(CategoryUpdateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            return View(vm);
        }
        var category = await _context.Categories.FindAsync(vm.Id);
        if (category == null)
            return NotFound();
        category.Name = vm.Name;
        
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
    
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
            return NotFound();
        
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(int id)
    {
        var category = await _context.Categories
            .Select(product=>new CategoryGetViewModel()
            {
                Id = product.Id,
                Name = product.Name
            }).FirstOrDefaultAsync(x => x.Id == id);
        
        if (category is null)
            return NotFound();
        return View(category);
    }
}