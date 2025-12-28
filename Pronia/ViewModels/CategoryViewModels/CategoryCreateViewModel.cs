using System.ComponentModel.DataAnnotations;

namespace Pronia.ViewModels.CategoryViewModels;

public class CategoryCreateViewModel
{
    [Required]
    public string Name { get; set; }
}