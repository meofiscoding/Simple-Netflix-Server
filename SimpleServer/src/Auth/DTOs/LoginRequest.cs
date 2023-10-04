using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleServer.src.Auth.DTOs
{
    public class LoginRequest
    {
        [MinLength(Consts.UsernameMinLength, ErrorMessage = Consts.UsernameLengthValidationError)]
        public string? Username { get; set; }

        [RegularExpression(Consts.PasswordRegex, ErrorMessage = Consts.PasswordValidationError)]
        public string? Password { get; set; }
    }
}
