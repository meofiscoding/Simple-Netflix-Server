namespace Identity.API.Domain.Mediator.Commands.Auth;

public class JwtConfiguration
{
    public const string Position = "JWT";

    public string? ValidAudience { get; set; }
    public string? ValidIssuer { get; set; }
    public string? Secret { get; set; }
}