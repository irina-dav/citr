using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using citr.Services;
using Microsoft.AspNetCore.Authorization;
using citr.Models.ViewModels;

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
            var result = ldapService.UpdateEmployees();
            /*var result = new ResultUserUpdate()
            {
                Errors = new List<string>() { "ошибка 1", "ошибка 2", "ошибка 3" },
                NewUserCount = 5,
                NotValidAccountCount = 3,
                SearchedAccountsCount = 10,
                UpdatedUserCount = 2,
                NewEmployees = new List<EmployeeViewModel>() { new EmployeeViewModel() { Account = "account1", Email = "email1", FullName = "fullname1", Position = "position1" } },
                NotValidAccounts = new List<string>() { "account1", "account2" },
                UpdatedEmployees = new List<EmployeeViewModel>() { new EmployeeViewModel() { Account = "account1", Email = "email1", FullName = "fullname1", Position = "position1" } }
            };*/
            return View("ResultUpdateEmployees", result);
        }
    }
}