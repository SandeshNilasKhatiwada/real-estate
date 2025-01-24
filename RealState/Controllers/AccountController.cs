using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealState.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RealState.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager,
                                 RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // Show Registration Page
        public IActionResult Register()
        {
            return View();
        }

        // Handle User Registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Debug: Check Role Assignment
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        var roleResult = await _roleManager.CreateAsync(new IdentityRole(model.Role));
                        if (!roleResult.Succeeded)
                        {
                            foreach (var error in roleResult.Errors)
                            {
                                Console.WriteLine($"Role Error: {error.Description}");
                                ModelState.AddModelError("", error.Description);
                            }
                            return View(model);
                        }
                    }

                    await _userManager.AddToRoleAsync(user, model.Role);

                    // Debug: Ensure user is signed in
                    Console.WriteLine($"User {user.Email} registered with role {model.Role}");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Properties");
                }

                // Log creation errors
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"User Creation Error: {error.Description}");
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Log ModelState errors if any
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
            }

            return View(model);
        }

        // Show Login Page
        public IActionResult Login()
        {
            return View();
        }

        // Handle User Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    Console.WriteLine($"User {model.Email} logged in successfully.");
                    return RedirectToAction("Index", "Properties");
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }

            // Log ModelState errors if any
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
            }

            return View(model);
        }

        // Logout User
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            Console.WriteLine("User logged out.");
            return RedirectToAction("Login", "Account");
        }
    }
}
