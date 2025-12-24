using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Pronia.Common;

namespace Pronia.Models;

public class Product:BaseEntity
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    [Precision(10,2)]
    public decimal Price { get; set; }
    public string ImagePath { get; set; }
    [Required]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }  
}