using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Models.Db;
using Shopping.Models.ViewModel;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using MimeKit;

namespace Shopping.Controllers
{
    public class AccountController : Controller
    {
        private OnlineShopContext _context;
        public AccountController(OnlineShopContext context)
        {
            _context = context; 
        }
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            user.FullName = user.FullName?.Trim();
            user.Email = user.Email?.Trim();
            user.Password = user.Password?.Trim();
            user.RegisterDate = DateTime.Now;
            user.IsAdmin = false;
            user.RecoveryCode = 0;
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(user.Email);
            if (!match.Success)
            {
                ModelState.AddModelError("Email", "Email is not valid");
                return View(user);
            }
            
            var PrevUser=_context.Users.Any(u => u.Email == user.Email);
            if (PrevUser)
            {
                ModelState.AddModelError("Email", "Email is already exist");
                return View(user);
            }

            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("login");
        }
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Login(LoginViewModel user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
           
            var GetUser = _context.Users.FirstOrDefault(u => u.Email == user.Email.Trim()&&u.Password== user.Password.Trim());
            if (GetUser == null)
            {
                ModelState.AddModelError("Email", "The Email or Password isn't correct");
                return View(user);
            }
            // Create claims for the authenticated user
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, GetUser.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, GetUser.FullName));
            claims.Add(new Claim(ClaimTypes.Email, GetUser.Email));
            if (GetUser.IsAdmin == true)
            {
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "user"));
            }
            // Create an identity based on the claims
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            // Create a principal based on the identity
            var principal = new ClaimsPrincipal(identity);
            // Sign in the user with the created principal
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return Redirect("/");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public async Task<IActionResult> RecoveryPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecoveryPassword(RecoveryPasswordViewModel recoveryPassword)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            ////-------------------------------------------

            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(recoveryPassword.Email);

            if (!match.Success)
            {
                ModelState.AddModelError("Email", "Email is not valid");
                return View(recoveryPassword);
            }

            var GetUser = _context.Users.FirstOrDefault(x => x.Email == recoveryPassword.Email.Trim());
            if (GetUser == null)
            {
                ModelState.AddModelError("Email", "Email is not exist");
                return View(recoveryPassword);
            }

            ////-------------------------------------------
            GetUser.RecoveryCode = new Random().Next(10000, 100000);
            _context.Users.Update(GetUser);
            _context.SaveChanges();

            ////-------------------------------------------

            MailMessage mail = new MailMessage();

            mail.From = new MailAddress("emailsendertest0055@gmail.com");
            mail.To.Add(GetUser.Email);
            mail.Subject = "Recovery code";
            mail.Body = "youre recovery code:" + GetUser.RecoveryCode;

            // إعداد SMTP Server
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential("emailsendertest0055@gmail.com", "fflf cwva cbmn bpgb");
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);

            //mailKit
         /*   var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Your Name", "your-email@gmail.com"));
            email.To.Add(new MailboxAddress("Recipient", "recipient@example.com"));
            email.Subject = "Test Email";
            email.Body = new TextPart("plain") { Text = "Hello! This is a test email from C# using MailKit." };

            using var smtp = new SmtpClient();
            smtp.Connect("smtp.gmail.com", 587);
            smtp.Authenticate("your-email@gmail.com", "your-password");
            smtp.Send(email);
            smtp.Disconnect(true);

            Console.WriteLine("📨 Email sent successfully!");
*/
            ////-------------------------------------------
            return Redirect("/Account/ResetPassword?email=" + GetUser.Email);
        }
        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            var resetPasswordModel = new ResetPasswordViewModel();
            resetPasswordModel.Email = email;

            return View(resetPasswordModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel resetPassword)
        {
            if (!ModelState.IsValid)
            {
                return View(resetPassword);
            }

            ////-------------------------------------------

            var foundUser = _context.Users.FirstOrDefault(x => x.Email == resetPassword.Email &&x.RecoveryCode==resetPassword.RecoveryCode);
            if (foundUser == null)
            {
                ModelState.AddModelError("RecoveryCode", "Email or recovery code is not valid");
                return View(resetPassword);
            }

            ////-------------------------------------------

            foundUser.Password = resetPassword.NewPassword;

            _context.Users.Update(foundUser);
            _context.SaveChanges();

            ////-------------------------------------------

            return RedirectToAction(nameof(Login));
        }
    }
}
