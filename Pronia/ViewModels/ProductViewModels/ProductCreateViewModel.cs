using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Pronia.ViewModels.ProductViewModels;

public class ProductCreateViewModel
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    [Precision(10,2)]
    public decimal Price { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    public IFormFile MainImage { get; set; }
    public IFormFile HoverImage { get; set; }
    public List<IFormFile> Images  { get; set; }
}