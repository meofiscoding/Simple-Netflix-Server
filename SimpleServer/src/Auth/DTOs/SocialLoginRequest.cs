using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleServer.src.Auth.DTOs
{
    public class SocialLoginRequest
    {
        [MinLength(Consts.UsernameMinLength, ErrorMessage = Consts.UsernameLengthValidationError)]
        public string? Email { get; set; }

        [Required] public string? Provider { get; set; }

        [Required] public string? AccessToken { get; set; }
    }
}
