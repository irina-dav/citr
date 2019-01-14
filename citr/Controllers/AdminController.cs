using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using citr.Services;
using Microsoft.AspNetCore.Authorization;

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

        public ViewResult PopulateEmployees()
        {
            var result = ldapService.PopulateEmployees().ToString();
            
            return View(model: result);
        }
    }
}