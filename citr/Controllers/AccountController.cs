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
using citr.Services;
using citr.Models.ViewModels;
using citr.Models;

namespace citr.Controllers
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

        public IActionResult AccessDeniedPath(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            TempData["warning"] = "У вас отсутствует доступ к данному разделу";
            return View("Login");
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
                        Employee empl = repository.Employees.FirstOrDefault(e => e.Account.Equals(model.Username));
                        if (empl == null)
                        {
                            empl = new Employee()
                            {
                                Account = model.Username,
                                Email = user.Email,
                                FullName = user.DisplayName,
                                Position = user.Position,
                                UserRoleID = 0
                            };
                            repository.SaveEmployee(empl);
                        }
                        else
                        {
                            if (empl.UserRoleID == 1)
                            {
                                userClaims.Add(new Claim(ClaimTypes.Role, "Admins"));
                            }
                            else
                            {
                                userClaims.Add(new Claim(ClaimTypes.Role, "Users"));
                            }
                        }
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