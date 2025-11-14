using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CinemaApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.SUPER_ADMIN_ROLE)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View(_userManager.Users);
        }
        public async Task<IActionResult> LockUnLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (await _userManager.IsInRoleAsync(user, SD.SUPER_ADMIN_ROLE))
            {
                TempData["error-notification"] = "Cannot block Super Admin account.";
                return RedirectToAction("Index");
            }

            user.LockoutEnabled = !user.LockoutEnabled;

            if (!user.LockoutEnabled)
                user.LockoutEnd = DateTime.UtcNow.AddDays(7);
            else
                user.LockoutEnd = null;

            await _userManager.UpdateAsync(user);

            TempData["success-notification"] = $"Blocked {user.FirstName} {user.LastName}.";

            return RedirectToAction("Index");
        }
    }
}
