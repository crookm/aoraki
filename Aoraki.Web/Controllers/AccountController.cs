using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<MongoUser> _signInManager;
        private readonly UserManager<MongoUser> _userManager;

        public AccountController(SignInManager<MongoUser> signInManager, UserManager<MongoUser> userManager)
        {
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
            if (user == null) return Unauthorized("unauthorized");

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: true);
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