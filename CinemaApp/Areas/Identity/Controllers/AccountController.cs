using CinemaApp.Models;
using CinemaApp.Repositories.IRepositories;
using CinemaApp.Utilities;
using CinemaApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Threading.Tasks;

namespace CinemaApp.Areas.Identity.Controllers
{
    [Area("Identity")]
    

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManger;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;
        public AccountController(UserManager<ApplicationUser> userManger, SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender, IRepository<ApplicationUserOTP> applicationUserOTPRepository)
        {
            _userManger = userManger;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOTPRepository = applicationUserOTPRepository;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
                return View(registerVM);
            var user = new ApplicationUser()
            {
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                Email = registerVM.Email,
                UserName = registerVM.UserName,
            };

            var result = await _userManger.CreateAsync(user, registerVM.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Code);
                }
                return View(registerVM);
            }
            // Send Confirmation Mail
            var token = await _userManger.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token, userId = user.Id },
                Request.Scheme);

            await _emailSender.SendEmailAsync(registerVM.Email, "CinemaApp - Confirm Your Email",
                $"<h1>Confirm Your Email By Clicking <a href={link}>Here</a></h1>");

            TempData["success-notification"] = "Create Account Successfully , Confirm your Email by Check Your Email box";
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            var user = await _userManger.FindByNameAsync(loginVM.UserNameOREmail) ??
                  await _userManger.FindByEmailAsync(loginVM.UserNameOREmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid UserName / Email Or Password.");
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    ModelState.AddModelError(string.Empty, "Too many attempts, Try again after 5 Min.");
                else if (!user.EmailConfirmed)
                    ModelState.AddModelError(string.Empty, "Please Confirm Your Email First.");
                else
                    ModelState.AddModelError(string.Empty, "Invalid UserName / Email Or Password ");
                return View(loginVM);
            }
            TempData["success-notification"] = "Login Successfully";
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            if (!ModelState.IsValid)
                return View(resendEmailConfirmationVM);

            var user = await _userManger.FindByNameAsync(resendEmailConfirmationVM.UserNameOREmail) ??
                  await _userManger.FindByEmailAsync(resendEmailConfirmationVM.UserNameOREmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid UserName / Email ");
                return View(resendEmailConfirmationVM);
            }

            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Email Already Confirmed!!");
                return View(resendEmailConfirmationVM);
            }


            // Send Confirmation Mail
            var token = await _userManger.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token, userId = user.Id },
                Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email!, "CinemaApp - Resend Confirm Your Email",
                $"<h1>Confirm Your Email By Clicking <a href={link}>Here</a></h1>");

            TempData["success-notification"] = "Send Email Successfully";
            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManger.FindByIdAsync(userId);

            if (user is null)
            {
                TempData["error-notification"] = "Invalid User Cred.";
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManger.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                TempData["error-notification"] = "Invalid OR Expired Token";
            else
                TempData["success-notification"] = "Confirm Email Successfully";

            return RedirectToAction(nameof(Login));
        }


        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM)
        {

            if (!ModelState.IsValid)
                return View(forgetPasswordVM);

            var user = await _userManger.FindByNameAsync(forgetPasswordVM.UserNameOREmail) ??
                  await _userManger.FindByEmailAsync(forgetPasswordVM.UserNameOREmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid UserName / Email ");
                return View(forgetPasswordVM);
            }

            var otp = new Random().Next(1000, 9999).ToString();

            var userOTPs = await _applicationUserOTPRepository.GetAsync(e => e.ApplicationUserId == user.Id);
            var totalCount = userOTPs.Count(e => (DateTime.UtcNow - e.CreateAt).TotalHours < 24);

            if (totalCount > 5)
            {
                ModelState.AddModelError(string.Empty, "Too Many Attempts,Please Try Again Later");
                return View(forgetPasswordVM);
            }
            else
            {

                await _applicationUserOTPRepository.AddAsync(new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ApplicationUserId = user.Id,
                    CreateAt = DateTime.UtcNow,
                    IsValid = true,
                    OTP = otp,
                    ValidTo = DateTime.UtcNow.AddMinutes(30),
                });
                await _applicationUserOTPRepository.CommitAsync();

                await _emailSender.SendEmailAsync(user.Email!, "CinemaApp - Forget Password!",
                 $"<h1>Use This OTP: {otp} To Validate Your Account.WARING DON'T SHARE IT.</h1>");

                TempData["success-notification"] = "OTP Sent Please Check Your Email.";

            }
            TempData["From-ForgetPassword"] = Guid.NewGuid().ToString();
            return RedirectToAction("ValidateOTP", new { userId = user.Id });

        }
        public IActionResult ValidateOTP(string userId)
        {
            if (TempData["From-ForgetPassword"] is null)
                return NotFound();
            return View(new ValidateOTPVM()
            {
                UserId = userId,
            });
        }
        [HttpPost]
        public async Task<IActionResult> ValidateOTP(ValidateOTPVM validateOTPVM)
        {
            if (!ModelState.IsValid)
                return View(validateOTPVM);

            var validOTP = await _applicationUserOTPRepository.GetOneAsync(e => e.ApplicationUserId == validateOTPVM.UserId &&
            e.IsValid && e.OTP == validateOTPVM.OTP && e.ValidTo > DateTime.UtcNow);

            if (validOTP is null)
            {
                TempData["error-notification"] = "Invalid OTP";
                return RedirectToAction(nameof(ValidateOTP), new { userId = validateOTPVM.UserId });
            }
            TempData["From-ValidateOTP"]=Guid.NewGuid().ToString();
            return RedirectToAction("NewPassword", new { userId = validateOTPVM.UserId });
        }
        public IActionResult NewPassword(string userId)
        {
            if(TempData["From-ValidateOTP"] is null)
                return NotFound();

            return View(new NewPasswordVM()
            {
                UserId = userId,
            });
        }
        [HttpPost]
        public async Task<IActionResult> NewPassword(NewPasswordVM newPasswordVM)
        {
            if (!ModelState.IsValid)
                return View(newPasswordVM);

            var user=await _userManger.FindByIdAsync(newPasswordVM.UserId);
            if (user is null)
            {
                TempData["error-notification"] = "User Not Found";
                return RedirectToAction(nameof(NewPassword), new { userId = newPasswordVM.UserId });
            }
            var token = await _userManger.GeneratePasswordResetTokenAsync(user);
            await _userManger.ResetPasswordAsync(user,token,newPasswordVM.Password);

            TempData["success-notification"] = "Change Password Successfully";
            return RedirectToAction(nameof(Login));
        }
    }
}
