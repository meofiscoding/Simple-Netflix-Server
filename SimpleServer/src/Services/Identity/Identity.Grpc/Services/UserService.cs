using Identity.Grpc.Entity;
using Microsoft.AspNetCore.Identity;
using Identity.Grpc.Protos;
using Grpc.Core;

namespace Identity.Grpc.Service
{
    public class UserService : PaymentProtoService.PaymentProtoServiceBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public override async Task<PaymentResponse> UpdateUserMembership(PaymentRequest request, ServerCallContext context)
        {
            // get user by email address
            var user = await _userManager.FindByEmailAsync(request.UserEmail);
            // var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return new PaymentResponse
                {
                    IsUpdateRoleSuccessSuccess = false,
                    Message = "User not found",
                    UserId = null
                };
            }

            var result = new IdentityResult();
            if (request.IsPaymentSuccess)
            {
                // if user have role user, then add role user
                var oldRole = await _userManager.GetRolesAsync(user);
                if (oldRole.Contains(UserRoles.User))
                {
                    await _userManager.RemoveFromRoleAsync(user, UserRoles.User);
                }
                result = await _userManager.AddToRoleAsync(user, UserRoles.Member);
            }
            else
            {
                // if have role member, then remove role member 
                var oldRole = await _userManager.GetRolesAsync(user);
                if (oldRole.Contains(UserRoles.Member))
                {
                    await _userManager.RemoveFromRoleAsync(user, UserRoles.Member);
                }
                result = await _userManager.AddToRoleAsync(user, UserRoles.User);
            }
            return new PaymentResponse
            {
                IsUpdateRoleSuccessSuccess = result.Succeeded,
                Message = result.Succeeded ? "User updated" : $"User can not be updated due to {result.Errors}",
                UserId = user.Id
            };
        }

    }
}
