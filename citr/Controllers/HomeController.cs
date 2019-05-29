using citr.Models;
using citr.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
