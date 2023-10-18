using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models.Auth;

public class RegisterViewModel
{
    public string ReturnUrl { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    [DataType("Password")]
    public string Password { get; set; } = string.Empty;
}