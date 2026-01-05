using System.ComponentModel.DataAnnotations;

namespace Pronia.ViewModels.UserViewModels;

public class LoginViewModel
{
    [Required,MaxLength(32),MinLength(3),EmailAddress]
    public string EmailAddress { get; set; } = string.Empty;
    [Required,MaxLength(256),MinLength(6),DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}