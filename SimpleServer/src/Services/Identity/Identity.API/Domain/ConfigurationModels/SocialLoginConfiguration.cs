namespace Identity.API.Domain.Mediator.Commands.Auth;

public class SocialLoginConfiguration
{
    public const string Position = "SocialLogin";

    public FacebookConfiguration? Facebook { get; set; }
    public GoogleConfiguration? Google { get; set; }
}