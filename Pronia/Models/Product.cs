using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Pronia.Common;

namespace Pronia.Models;

public class Product:BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }  
    public string MainImagePath  { get; set; }
    public string HoverImagePath { get; set; }
    public ICollection<ProductImage> ProductImages { get; set; }    
    [Range(0,5)]
    public double Rating { get; set; }
    public ICollection<ProductTag> ProductTags { get; set; } = [];
    
}