using System;
using MediatR;
using Microsoft.AspNetCore.Identity;
using MongoConnector.Models;
using SimpleServer.src;
using SimpleServer.src.Auth.DTOs;

namespace SimpleServer.Utils.Mediator.Commands.Auth
{
    public class RegisterUserCommand : IRequest<RegisterResponseDto>
    {
        public RegisterUserCommand(RegisterRequest request)
        {
            Request = request;
        }

        public RegisterRequest Request { get; set; }

        public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResponseDto>
        {
            private readonly UserManager<Account> _userManager;

            public RegisterUserCommandHandler(UserManager<Account> userManager)
            {
                _userManager = userManager;
            }

            public async Task<RegisterResponseDto> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
            {
                var userByEmail = await _userManager.FindByEmailAsync(command.Request.Email!);
                var userByUsername = await _userManager.FindByNameAsync(command.Request.Username!);

                if (userByEmail is not null || userByUsername is not null)
                {
                    throw new Exception($"Account with email {command.Request.Email} or username {command.Request.Username} already exists.");
                }

                Account acc = new()
                {
                    Email = command.Request.Email,
                    UserName = command.Request.Username,
                    Provider = Consts.LoginProviders.Password,
                };

                var result = await _userManager.CreateAsync(acc, command.Request.Password!);

                await _userManager.AddToRoleAsync(acc, Role.User);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        $"Unable to register user {command.Request.Username}, errors: {GetErrorsText(result.Errors)}");
                }

                return new RegisterResponseDto
                {
                    Message = $"User {command.Request.Username} registered successfully."
                };
            }

            private static string GetErrorsText(IEnumerable<IdentityError> errors)
            {
                return string.Join(", ", errors.Select(error => error.Description).ToArray());
            }
        }
    }
}
