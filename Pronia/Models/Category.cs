using System.ComponentModel.DataAnnotations;
using Pronia.Common;

namespace Pronia.Models;

public class Category: BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
}