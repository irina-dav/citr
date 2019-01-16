using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using citr.Models;
using citr.Services;
using citr.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace citr.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILdapService ldapService;
        private readonly IRequestRepository requestRepository;

        public HomeController(ILdapService ldapService, IRequestRepository requestRepository)
        {
            this.ldapService = ldapService;
            this.requestRepository = requestRepository;
        }

        [Authorize]
        public IActionResult Index()
        {
            int currEmployeeId = ldapService.GetUserEmployee().EmployeeID;
            if (requestRepository.Requests
                .Any(r => r.State == RequestState.Approving && 
                    r.Details.Any(d => d.ResourceOwnerID == currEmployeeId && d.ApprovingResult == ResourceApprovingResult.None)))
            {
                return RedirectToAction("ListToApprove", "Request");
            }
            else if (User.IsInRole("Admins"))
            {
                return RedirectToAction("List", "Resource");
            }
            else
            {
                return RedirectToAction("ListMyRequests", "Request");
            }

        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
