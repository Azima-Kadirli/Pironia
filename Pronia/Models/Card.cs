using System.ComponentModel.DataAnnotations;

namespace Pronia.Models
{
    public class Card
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Icon field is required")]
        public string Icon { get; set; }

        [Required(ErrorMessage = "Description field is required")]
        [MinLength(3)]
        [MaxLength(15)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Title field is required")]
        [MaxLength(20)]
        public string Title { get; set; }
    }
}
