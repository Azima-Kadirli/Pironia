using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DiaSymReader;
using Pronia.ViewModels.UserViewModels;

namespace Pronia.Controllers;

public class AccountController(UserManager<AppUser>_userManager,SignInManager<AppUser>_signInManager,RoleManager<IdentityRole>_roleManager) : Controller
{
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var existUserName = await _userManager.FindByNameAsync(vm.UserName);
        if (existUserName is { })
        {
            ModelState.AddModelError("UserName", "Username  is already taken");
            return View(vm);
        }
        
        var existUserEmail = await _userManager.FindByEmailAsync(vm.EmailAddress);
        if (existUserEmail is { })
        {
            ModelState.AddModelError("EmailAddress", "Email Address is already taken");
            return View(vm);
        }
        
        
        AppUser user = new()
        {
            FullName = vm.FirstName + " " + vm.LastName,
            Email = vm.EmailAddress,
            UserName = vm.UserName
        };
        var result = await _userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(" ", error.Description);
            }
            return View(vm);
        }
        await _signInManager.SignInAsync(user, false);
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);
        var user = await _userManager.FindByEmailAsync(vm.EmailAddress);
        if (user is null)
        {
            ModelState.AddModelError("","Email or password is incorrect");
            return View(vm);
        }

        var loginresult = await _userManager.CheckPasswordAsync(user, vm.Password);
        if (!loginresult)
        {
            ModelState.AddModelError("","Email or password is incorrect.");
            return View(vm);
        }
        await  _signInManager.SignInAsync(user,vm.IsRemember);
        return RedirectToAction("Index", "Home");
    }
    
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    // public async Task<IActionResult> CreateRoles()
    // {
    //    await _roleManager.CreateAsync(new IdentityRole()
    //     {
    //         Name = "User"
    //     });
    //     await _roleManager.CreateAsync(new IdentityRole()
    //     {
    //         Name = "Admin"
    //     });
    //     await _roleManager.CreateAsync(new IdentityRole()
    //     {
    //         Name = "Moderator"
    //     });
    //     return Ok("Roles created");
    // }
}