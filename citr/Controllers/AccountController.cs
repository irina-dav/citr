using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RequestsAccess.Services;
using RequestsAccess.Models.ViewModels;
using RequestsAccess.Models;

namespace RequestsAccess.Controllers
{
    public class AccountController : Controller
    {
        private readonly IEmployeeRepository repository;
        private readonly ILdapService _authService;

        public AccountController(ILdapService authService, IEmployeeRepository repo)
        {
            _authService = authService;
            repository = repo;
        }

        public IActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }

     
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = _authService.Login(model.Username, model.Password);
                    /*var user = new AppUser()
                    {
                        DisplayName = model.Username,
                        Username = model.Username
                    };*/
                    if (null != user)
                    {
                        var userClaims = new List<Claim>
                    {
                        new Claim("displayName", user.DisplayName),
                        new Claim("username", user.Username),
                        new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username)
                    };
                        /* if (user.IsAdmin)
                         {
                             userClaims.Add(new Claim(ClaimTypes.Role, "Admins"));
                         }*/
                        var principal = new ClaimsPrincipal(new ClaimsIdentity(userClaims, _authService.GetType().Name, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType));
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        return Redirect(returnUrl ?? "/");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
            
        }
    }
}