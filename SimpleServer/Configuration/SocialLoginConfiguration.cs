using System;

namespace SimpleServer.Configuration
{
    public class SocialLoginConfiguration 
    {
        public const string Position = "SocialLogin";

        public GoogleConfiguration? Google { get; set; }

        public FacebookConfiguration? Facebook { get; set; }
    }
}
