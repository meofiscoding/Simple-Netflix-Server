using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Movie.FunctionalTests
{
    class AutoAuthorizeMiddleware
    {
        // GUID representing the identity of the user
        public const string IDENTITY_ID = "9e3163b9-1ae6-4652-9dc6-7898ab7b7a00";

        private readonly RequestDelegate _next;

        // Configure the middleware and set the next step in the request pipeline.
        public AutoAuthorizeMiddleware(RequestDelegate rd)
        {
            _next = rd;
        }

        // Automatically authorizes a user by attaching specific claims to their identity
        public async Task Invoke(HttpContext httpContext)
        {
            // Simulate an authenticated user with the provided identity ID
            var identity = new ClaimsIdentity("cookies");

            identity.AddClaim(new Claim("sub", IDENTITY_ID));
            identity.AddClaim(new Claim("unique_name", IDENTITY_ID));
            identity.AddClaim(new Claim(ClaimTypes.Name, IDENTITY_ID));

            httpContext.User.AddIdentity(identity);

            // Ensures that the request continues to be processed by subsequent middleware components 
            // after the claims have been added to the identity.
            await _next.Invoke(httpContext);
        }
    }
}