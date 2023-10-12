namespace Identity.API.Models.Auth;

public class LoginInputModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
}