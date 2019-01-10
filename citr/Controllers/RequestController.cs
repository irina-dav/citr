using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RequestsAccess.Infrastructure;
using RequestsAccess.Models;
using RequestsAccess.Models.ViewModels;
using RequestsAccess.Services;
using Microsoft.AspNetCore.Http.Extensions;

namespace RequestsAccess.Controllers
{
    [Authorize]
    public class RequestController : Controller
    {
        private IRequestRepository repository;
        private IEmployeeRepository employeeesRepository;
        private IResourceRepository resourcesRepository;
        private IMailService mailService;
        private ILdapService ldapService;
        private readonly HistoryService historyService;
        private readonly ApplicationDbContext context;

        public RequestController(
            IEmployeeRepository employeeRepo,
            IResourceRepository resourceRepo, 
            IRequestRepository repo, 
            IMailService mailSrv, 
            ILdapService ldapSrv,
            HistoryService historySrv,
            ApplicationDbContext ctx)
        {
            employeeesRepository = employeeRepo;
            resourcesRepository = resourceRepo;
            repository = repo;
            mailService = mailSrv;
            ldapService = ldapSrv;
            historyService = historySrv;
            context = ctx;
        }

        public ViewResult ListAll()
        {
            ViewBag.Title = "Все заявки";
            return View("List", repository.Requests);
        }

        public ViewResult List(IEnumerable<Request> requests)
        {
            return View("List", requests);
        }

        [Authorize]
        public ViewResult ListMyRequests()
        {
            Employee currEmployee = ldapService.GetUserEmployee();
            ViewBag.Title = "Мои заявки";
            return View("List", repository.Requests.Where(r => r.AuthorID == currEmployee.EmployeeID));
        }

        [Authorize]
        public ViewResult ListToApprove()
        {
            Employee currEmployee = ldapService.GetUserEmployee();
            ViewBag.Title = "Заявки мне на согласование";
            return View("List", repository.Requests
                .Where(r => r.State == RequestState.Approving && r.ResourceAccesses.Any(ra => ra.Resource.OwnerEmployeeID == currEmployee.EmployeeID)));
        }

        public IActionResult Create()
        {
            return View("Edit", new Request()
            {
                State = RequestState.New,      
                Author = ldapService.GetUserEmployee(),
                AuthorID = ldapService.GetUserEmployee().EmployeeID
            });
        }

        //[Route("Request/Approve", Order = 1)]
        //[Route("Request/Edit", Order = 0)]        
        public IActionResult Edit(int requestId)
        {
            Request req = repository.Requests.FirstOrDefault(p => p.RequestID == requestId);
            Employee currentEmployee = ldapService.GetUserEmployee();
            int currentEmployeeId = currentEmployee.EmployeeID;
            if (req.AuthorID == currentEmployee.EmployeeID || req.ResourceAccesses.Any(ra => ra.Resource.OwnerEmployeeID == currentEmployeeId))
            {
                foreach (ResourceAccess ra in req.ResourceAccesses)
                {
                    ra.CanApprove = (req.State == RequestState.Approving && ra.Resource.OwnerEmployeeID == currentEmployeeId);
                    ra.CanDelete = req.State == RequestState.New;
                }
                foreach (EmployeeAccess ea in req.EmployeeAccesses)
                {
                    ea.CanDelete = (req.State == RequestState.New) && req.AuthorID == currentEmployeeId;
                    
                }
                return View(req);
            }
            else
                return View("~/Views/Errors/AccessDenied.cshtml");
        }

       [HttpPost]
       public IActionResult Edit(Request model)
        {
            Request req = SaveRequstPost(model);
            if (req == null)
            {
                return View(model);
            }
            else
            {
                return RedirectToAction("ListMyRequests");
            }
        }

        public IActionResult Cancel()
        {
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public ActionResult AddEmployeeAccess(int index, string employeeId, string accessLevel)
        {
            int emplId = int.Parse(employeeId);
            var newObj = new EmployeeAccess()
            {
                EmployeeID = emplId,
                AccessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), accessLevel, true),
                Employee = employeeesRepository.Employees.FirstOrDefault(e => e.EmployeeID.Equals(emplId))
             };
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("EmployeeAccesses[{0}]", index);
            return PartialView("~/Views/Request/AddEmployeeAccess.cshtml", newObj);
        }

        [HttpPost]
        public ActionResult AddResource(int index, string resourceId)
        {
            int resId = int.Parse(resourceId);
            var newObj = new ResourceAccess()
            {
                ResourceID = resId,
                Resource = resourcesRepository.Resources.FirstOrDefault(e => e.ResourceID.Equals(resId))
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("ResourceAccesses[{0}]", index);
            return PartialView("~/Views/Request/AddResource.cshtml", newObj);
        }

        [HttpPost]
        public IActionResult Send(Request model)        
        {
            Request req = SaveRequstPost(model);
            if (req == null)
            {
                return View("Edit", model);
            }
            else
            {
                req.State = RequestState.Approving;
                repository.SaveRequest(req);
                string mess = $"Заявка {req.RequestID} была отправлена на согласование";                
                historyService.AddRow(req, mess);
                TempData["message"] = mess;
                foreach (Employee empl in req.ResourceAccesses.Select(ra => ra.Resource.OwnerEmployee).Distinct())
                {
                    SendEmail(req, empl);
                }
                return RedirectToAction("ListMyRequests");
            }
        }


        public async void SendEmail(Request req, Employee recipient)
        {
            EmailViewModel model = new EmailViewModel()
            {
                Recipient = recipient,
                Request = req,
                Resources = req.ResourceAccesses.Select(ra => ra.Resource).Where(r => r.OwnerEmployeeID == recipient.EmployeeID),
                Url = this.AbsoluteAction("Approve", "Request", new { requestId = req.RequestID })
            };
            var viewHtml = await this.RenderViewAsync("Approve", model, true);
            System.IO.File.WriteAllText(@"d:/temp/test.html", viewHtml);
            
            await mailService.SendEmailAsync("i.davydenko@pharmasyntez.com", $"Согласование заявки на доступ №{req.RequestID}", viewHtml);
        }


        public IActionResult Test()
        {
            mailService.SendEmailAsync("i.davydenko@pharmasyntez.com",
               $"test",
               "Согласуйте, пожалуйста заявку");
            return RedirectToAction("List");

        }


        [HttpPost]
        public IActionResult EndApprove(Request model)
        {
            Request req = SaveRequstPost(model);
            string mess = "";
            if (req.ResourceAccesses.Any(ra => ra.ApprovingResult == ResourceApprovingResult.None))
            {
                mess = $"Согласование заявки {req.RequestID} завершено";              
            }
            else
            {
                if (!req.ResourceAccesses.Any(ra => ra.ApprovingResult == ResourceApprovingResult.Approved))
                {
                    req.State = RequestState.Canceled;
                    mess = $"Заявке {req.RequestID} было отказано в согласовании";
                }
                else
                {
                    req.State = RequestState.Approved;
                    mess = $"Заявка {req.RequestID} согласована";
                }
                repository.SaveRequest(req);
            }
               
            historyService.AddRow(req, mess);
            TempData["message"] = mess;
            return RedirectToAction("ListToApprove");         
        }

        [HttpPost]
        public IActionResult Approve(string requestId, string commentApproving)
        {
            Request req = repository.Requests.First(r => r.RequestID == int.Parse(requestId));
            req.State = RequestState.Approved;
            repository.SaveRequest(req);
            string comment = string.IsNullOrEmpty(commentApproving) ? "" : $". Комментарий: {commentApproving}";
            string mess = $"Заявка {req.RequestID} была согласована{comment}";
            historyService.AddRow(req, mess);
            TempData["message"] = mess;
            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult NotApprove(string requestId, string commentApproving)
        {
            Request req = repository.Requests.First(r => r.RequestID == int.Parse(requestId));
            req.State = RequestState.NotApproved;
            repository.SaveRequest(req);
            string comment = string.IsNullOrEmpty(commentApproving) ? "" : $". Комментарий: {commentApproving}";
            string mess = $"Заявке {req.RequestID} было отказано в согласовании{comment}";
            historyService.AddRow(req, mess);
            TempData["message"] = mess;
            return RedirectToAction("List");
        }

        private Request SaveRequstPost(Request model)
        {
            var employeeAccesses = model.EmployeeAccesses?.Where(c => !c.IsDeleted).ToList();
            var resourceAccesses = model.ResourceAccesses?.Where(c => !c.IsDeleted).ToList();

            if (ModelState.IsValid)
            {
                bool isNew = model.RequestID == 0;
                if (isNew)
                {   
                    model.AuthorID = ldapService.GetUserEmployee().EmployeeID;
                    model.CreateDate = DateTime.Now;
                    model.State = RequestState.New;
                }        
                string mess = "";

                model.EmployeeAccesses = employeeAccesses;
                model.ResourceAccesses = resourceAccesses;
                model.ChangeDate = DateTime.Now;
                repository.SaveRequest(model);
                Request req = repository.Requests.First(r => r.RequestID.Equals(model.RequestID));
                if (isNew)
                    mess = $"Заявка {req.RequestID} была создана";
                else
                    mess = $"Заявка {req.RequestID} был сохранeна";
                historyService.AddRow(req, mess);
                TempData["message"] = mess;
                return req;
            }
            else
            {
                if (employeeAccesses != null)
                    employeeAccesses.ForEach(ea => ea.Employee = employeeesRepository.Employees.First(em => em.EmployeeID.Equals(ea.EmployeeID)));
                if (resourceAccesses != null)
                    resourceAccesses.ForEach(ra => ra.Resource = resourcesRepository.Resources.First(r => r.ResourceID.Equals(ra.ResourceID)));
                model.EmployeeAccesses = employeeAccesses;
                model.ResourceAccesses = resourceAccesses;
                model.Author = employeeesRepository.Employees.FirstOrDefault(e => e.EmployeeID == model.AuthorID);
                return null;
            }
        }




        /*[HttpPost]
        public IActionResult Delete(int resourceId)
        {
            Resource deletedResource = repository.DeleteResource(resourceId);
            if (deletedResource != null)
            {
                TempData["message"] = $"{deletedResource.Name} был удалён";
            }
            return RedirectToAction("List");
        }*/
    }
}
