using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Identity.API.Models.Auth;
using Identity.API.Entity;

namespace Identity.API.Controllers;

public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interactionService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(SignInManager<ApplicationUser> signInManager, IIdentityServerInteractionService interactionService, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _interactionService = interactionService;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl)
    {
        return View(new LoginViewModel() { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        // get tenant info// check if the model is valid
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(vm.Username!) ?? await _userManager.FindByEmailAsync(vm.Username!);
            // check if the user exists
            if (user != null)
            {
                // check if the password is correct
                var signInResult = _signInManager.PasswordSignInAsync(user, vm.Password, false, false).Result;
                if (signInResult.Succeeded)
                {
                    // redirect to the return url
                    if (vm.ReturnUrl != null)
                    {
                        return Redirect(vm.ReturnUrl);
                    }
                    else
                    {
                        return View();
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Username or password is incorrect");
            }
        }
        return Redirect(vm.ReturnUrl);
    }

    [HttpGet]
    public async Task<IActionResult> Logout(string logoutId)
    {
        await _signInManager.SignOutAsync();

        var logoutRequest = await _interactionService.GetLogoutContextAsync(logoutId);

        if (string.IsNullOrEmpty(logoutRequest.PostLogoutRedirectUri))
        {
            return RedirectToAction("Index", "Home");
        }

        return Redirect(logoutRequest.PostLogoutRedirectUri);
    }

    [HttpGet]
    public IActionResult Register(string returnUrl)
    {
        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var userByEmail = await _userManager.FindByEmailAsync(vm.Email!);
        var userByUsername = await _userManager.FindByNameAsync(vm.Username!);
        if (userByEmail is not null || userByUsername is not null)
        {
            throw new Exception($"User with email {vm.Email} or username {vm.Username} already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = vm.Username,
            Email = vm.Email
        };

        var result = await _userManager.CreateAsync(user, vm.Password);
        // await _userManager.AddToRoleAsync(user, "User");

        if (!result.Succeeded)
        {
            throw new Exception($"User creation failed. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await _signInManager.SignInAsync(user, false);

        return Redirect(vm.ReturnUrl);
    }

}