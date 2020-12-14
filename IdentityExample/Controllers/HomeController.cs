using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using System.Threading.Tasks;

namespace IdentityExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailService _emailService;

        public HomeController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        // GET
        public IActionResult Index()                // homepage
        {
            return View();
        }

        [Authorize]
        public IActionResult Secret()               // secret page, only accesible after successfull login
        {
            return View();
        }
        public IActionResult Login()                // login page
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            //login functionality
            var user = await _userManager.FindByNameAsync(username);

            if (user != null)
            {
                //sign in
                var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Index");               // redirect to homepage
                }
            }

            return RedirectToAction("Index");                   // redirect to homepage
        }

        //register page
        public IActionResult Register()
        {
            return View();
        }

        // register request handler
        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            //register functionality
            var user = new IdentityUser
            {
                UserName = username,
                Email = "",
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // email verification
                //generation of the email token
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // email verification link
                var link = Url.Action(nameof(VerifyEmail), "Home", new { userId = user.Id, code }, Request.Scheme, Request.Host.ToString());

                // send confirmation email
                await _emailService.SendAsync("test@test.com", "email verify", $"<a href=\"{link}\">Verify Email</a>", true);

                return RedirectToAction("EmailVerification");           // redirect to email verification page
            }

            return RedirectToAction("Index");               // redirect to home page
        }

        // email verification
        public async Task<IActionResult> VerifyEmail(string userId, string code)
        {
            // get user
            var user = await _userManager.FindByIdAsync(userId);

            // if user not found raise error
            if (user == null) return BadRequest();

            // confirm email
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return View();
            }

            return BadRequest();
        }

        public IActionResult EmailVerification() => View();

        // logout the user
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            // redirect to home page
            return RedirectToAction("Index");
        }
    }
}