using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Aoraki.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(ILogger<AccountController> logger, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction(nameof(AdminController.Index), "admin");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([Required] string email, [Required] string password, string redirectUrl)
        {
            var user = await _userManager.FindByNameAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Person attempted to authorise with email {0}, but failed as it does not exist", email);
                return Unauthorized("unauthorized");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: true);
                _logger.LogInformation("Person successfully authorised with email {0}", email);
                if (string.IsNullOrEmpty(redirectUrl))
                    return RedirectToAction(nameof(AdminController.Index), "admin");
                else return Redirect(redirectUrl);
            }
            
            if (result.IsLockedOut)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                return Unauthorized($"locked out, ends: {lockoutEnd}");
            }

            return Unauthorized("unauthorized");
        }
    }
}