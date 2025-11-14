using CinemaApp.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CinemaApp.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _usermanager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _usermanager=userManager;
        }
        
        public async Task<IActionResult> Index()
        {
            var user =await _usermanager.GetUserAsync(User);

            if (user is null) 
                return NotFound();

            //var userVM = new ApplicationUserVM()
            //{
            //    FullName =$"{user.FirstName} {user.LastName}" ,
            //    Address = user.Address,
            //    Email = user.Email,
            //    PhoneNumber = user.PhoneNumber
            //};

         
            var userVM = user.Adapt<ApplicationUserVM>();

            return View(userVM);
        }
        public async Task<IActionResult> UpdateProfile(ApplicationUserVM applicationUserVM)
        {
            var user = await _usermanager.GetUserAsync(User);

            if (user is null)
                return NotFound();
           
            var names=applicationUserVM.FullName.Split(" ");

            user.PhoneNumber = applicationUserVM.PhoneNumber;
            user.Address=applicationUserVM.Address;
            user.FirstName = names[0];
            user.LastName = names[1];

            await _usermanager.UpdateAsync(user);

            TempData["success-notification"] = "Update Profile Successfully";
            return RedirectToAction(nameof(Index));

        }
        public async Task<IActionResult> UpdatePassword(ApplicationUserVM applicationUserVM)
        {
            var user = await _usermanager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            if(applicationUserVM.CurrentPassword is null || applicationUserVM.NewPassword is null)
            {
                TempData["error-notification"] = "Must Have a CurrentPassword & NewPassword Value.";
                return RedirectToAction(nameof(Index));

            }

            var result= await _usermanager.ChangePasswordAsync(user,
               applicationUserVM.CurrentPassword,applicationUserVM.NewPassword);


            if (!result.Succeeded)
            {
                TempData["error-notification"] =String.Join( " ",result.Errors.Select(e=>e.Code));
                return RedirectToAction(nameof(Index));

            }

            TempData["success-notification"] = "Update Profile Successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
