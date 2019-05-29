using citr.Models.ViewModels;
using citr.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace citr.Controllers
{
    [Authorize(Roles = "Admins")]
    public class AdminController : Controller
    {
        private ILdapService ldapService;

        public AdminController(ILdapService ldapServ)
        {
            ldapService = ldapServ;
        }

        public ViewResult StartUpdateEmployees()
        {
            return View();
        }


        [HttpPost]
        public IActionResult UpdateEmployees()
        {
            ResultUserUpdate result = ldapService.UpdateEmployees();
            return View("ResultUpdateEmployees", result);
        }
    }
}