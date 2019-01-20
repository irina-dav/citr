using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using citr.Infrastructure;
using citr.Models;
using citr.Models.ViewModels;
using citr.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace citr.Controllers
{
    [Authorize]
    public class RequestController : Controller
    {
        private IRequestRepository repository;
        private IEmployeeRepository employeeesRepository;
        private IResourceRepository resourcesRepository;
        private IAccessRoleRepository roleRepository;

        private readonly ILdapService ldapService;
        private readonly OTRSService otrsService;
        private readonly HistoryService historyService;
        private readonly NotificationService notifService;
        private readonly ApplicationDbContext context;

        private readonly ILogger logger;

        public RequestController(
            IEmployeeRepository employeeRepo,
            IResourceRepository resourceRepo, 
            IRequestRepository repo, 
            IAccessRoleRepository roleRepo,
            ILdapService ldapSrv,
            HistoryService historySrv,
            ApplicationDbContext ctx,
            OTRSService otrsServ,
            ILogger<RequestController> log,
            NotificationService notifService)
        {
            employeeesRepository = employeeRepo;
            resourcesRepository = resourceRepo;
            roleRepository = roleRepo;
            repository = repo;
            ldapService = ldapSrv;
            historyService = historySrv;
            context = ctx;
            logger = log;
            otrsService = otrsServ;
            this.notifService = notifService;
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
            var requests = repository.Requests.Where(r => r.AuthorID == currEmployee.EmployeeID).ToList();       
            return View("List", requests);
        }

        [Authorize]
        public ViewResult ListToApprove()
        {
            Employee currEmployee = ldapService.GetUserEmployee();
            ViewBag.Title = "Заявки мне на согласование";
            return View("List", repository.Requests
                .Where(r => r.State == RequestState.Approving && r.Details.Any(d => d.ResourceOwnerID == currEmployee.EmployeeID)));
        }

        /* public IActionResult Create()
         {
             ViewBag.CurrentEmployeeId = ldapService.GetUserEmployee().EmployeeID;
             return View("Edit", new Request()
             {
                 State = RequestState.New,      
                 Author = ldapService.GetUserEmployee(),
                 AuthorID = ldapService.GetUserEmployee().EmployeeID
             });
         }*/

        public IActionResult Create()
        {
            return View("Edit", new RequestViewModel()
            {
                State = RequestState.New,
                Author = ldapService.GetUserEmployee(),
                //AuthorID = ldapService.GetUserEmployee().EmployeeID
            });
        }

        /*public ViewResult Copy(int sourceId)
        {
            Request req = repository.Requests.FirstOrDefault(p => p.RequestID == sourceId);
            Request newReq = new Request()
            {
                State = RequestState.New,
                Author = ldapService.GetUserEmployee(),
                AuthorID = ldapService.GetUserEmployee().EmployeeID,
                Comment = req.Comment,
                Details = new List<RequestDetail>(req.Details.Select(d => new RequestDetail()
                {
                    Resource = d.Resource,
                    ResourceID = d.ResourceID,
                    ResourceOwner = d.ResourceOwner,
                    EmployeeAccess = d.EmployeeAccess,
                    EmployeeAccessID = d.EmployeeAccessID,
                    ResourceOwnerID = d.Resource.OwnerEmployee.EmployeeID,
                    RoleID = d.RoleID,    
                    Role = d.Role
                }))
            };
            ViewBag.CurrentEmployeeId = ldapService.GetUserEmployee().EmployeeID;
            return View("Edit", newReq);
        }*/

        public ViewResult Copy(int sourceId)
        {
            Request req = repository.Requests.FirstOrDefault(p => p.RequestID == sourceId);
            RequestViewModel newReqModel = new RequestViewModel()
            {
                State = RequestState.New,
                Author = ldapService.GetUserEmployee(),
                //AuthorID = ldapService.GetUserEmployee().EmployeeID,
                Comment = req.Comment,
                Details = new List<RequestDetailViewModel>(req.Details.Select(d => new RequestDetailViewModel()
                {
                    Resource = d.Resource,
                    ResourceID = d.ResourceID,
                    ResourceOwner = d.ResourceOwner,
                    EmployeeAccess = d.EmployeeAccess,
                    EmployeeAccessID = d.EmployeeAccessID,
                    ResourceOwnerID = d.Resource.OwnerEmployee.EmployeeID,
                    RoleID = d.RoleID,
                    Role = d.Role
                }))
            };
            //ViewBag.CurrentEmployeeId = ldapService.GetUserEmployee().EmployeeID;           
            return View("Edit", newReqModel);
        }
        /*
                public IActionResult Open(int requestId)
                {
                    Request req = repository.Requests.FirstOrDefault(p => p.RequestID == requestId);
                    Employee currentEmployee = ldapService.GetUserEmployee();
                    int currentEmployeeId = currentEmployee.EmployeeID;
                    ViewBag.CurrentEmployeeId = ldapService.GetUserEmployee().EmployeeID;
                    if (req.AuthorID == currentEmployee.EmployeeID || req.Details.Any(ra => ra.Resource.OwnerEmployeeID == currentEmployeeId))
                    {
                        foreach (RequestDetail rd in req.Details)
                        {
                            rd.CanApprove = (req.State == RequestState.Approving && rd.ResourceOwnerID == currentEmployeeId);
                            rd.CanDelete = req.State == RequestState.New;
                        }
                        return View("Edit", req);
                    }
                    else
                        return View("~/Views/Errors/AccessDenied.cshtml");
                }*/

        public IActionResult Open(int requestId)
        {
            Request req = repository.Requests.FirstOrDefault(p => p.RequestID == requestId);
            RequestViewModel requestModel = new RequestViewModel(req);
            Employee currentEmployee = ldapService.GetUserEmployee();
            int currentEmployeeId = currentEmployee.EmployeeID;
            if (requestModel.Author.EmployeeID == currentEmployee.EmployeeID || requestModel.Details.Any(ra => ra.Resource.OwnerEmployeeID == currentEmployeeId))
            {
                foreach (RequestDetailViewModel rd in requestModel.Details)
                {
                    rd.CanApprove = (req.State == RequestState.Approving && rd.ResourceOwnerID == currentEmployeeId);
                    rd.CanDelete = req.State == RequestState.New;
                }
                return View("Edit", requestModel);
            }
            else
                return View("~/Views/Errors/AccessDenied.cshtml");
        }

        /* [HttpPost]
         public IActionResult Edit2(Request model)
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
          }*/

        [HttpPost]
        public IActionResult Edit(RequestViewModel model)
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


       /* [HttpPost]
        public ActionResult AddDetail2(int index, string resourceId, string employeeId, string roleId)
        {
            int resId = int.Parse(resourceId);
            int accessRoleId = int.Parse(roleId);
            int emplId = int.Parse(employeeId);
            Resource res = resourcesRepository.Resources.FirstOrDefault(e => e.ResourceID.Equals(resId));
            Employee empl = employeeesRepository.Employees.FirstOrDefault(e => e.EmployeeID.Equals(emplId));
            var newObj = new RequestDetail()
            {
                ResourceID = resId,
                Resource = res,
                ResourceOwnerID = res.OwnerEmployee.EmployeeID,
                ResourceOwner = res.OwnerEmployee,
                EmployeeAccess = empl,
                EmployeeAccessID = emplId,
                RoleID = accessRoleId,
                Role = roleRepository.Roles.FirstOrDefault(e => e.ID.Equals(accessRoleId))
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("Details[{0}]", index);
            return PartialView("~/Views/Request/Detail.cshtml", newObj);
        }*/

        [HttpPost]
        public ActionResult AddDetail(int index, string resourceId, string employeeId, string roleId)
        {
            int resId = int.Parse(resourceId);
            int accessRoleId = int.Parse(roleId);
            int emplId = int.Parse(employeeId);
            Resource res = resourcesRepository.Resources.FirstOrDefault(e => e.ResourceID.Equals(resId));
            Employee empl = employeeesRepository.Employees.FirstOrDefault(e => e.EmployeeID.Equals(emplId));
            var newObj = new RequestDetailViewModel()
            {
                ResourceID = resId,
                Resource = res,
                ResourceOwnerID = res.OwnerEmployee.EmployeeID,
                ResourceOwner = res.OwnerEmployee,
                EmployeeAccess = empl,
                EmployeeAccessID = emplId,
                RoleID = accessRoleId,
                Role = roleRepository.Roles.FirstOrDefault(e => e.ID.Equals(accessRoleId))
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("Details[{0}]", index);
            return PartialView("~/Views/Request/Detail.cshtml", newObj);
        }

        /* [HttpPost]
         public async Task<IActionResult> SendToApprove(Request model)        
         {
             /*Request req = SaveRequstPost(model);
             if (req == null)
             {
                 return View("Edit", model);
             }
             else
             {
                 req.State = RequestState.Approving;
                 repository.SaveRequest(req);
                 string mess = $"Заявка <b>{req.RequestID}</b> была отправлена на согласование";                
                 historyService.AddRow(req, mess);
                 TempData["message"] = mess;               
                 await notifService.SendToApprovers(req);                
                 return RedirectToAction("ListMyRequests");
             }

         }*/

        public async Task<IActionResult> SendToApprove(RequestViewModel model)
        {
            Request req = SaveRequstPost(model);
            if (req == null)
            {
                return View("Edit", model);
            }
            else
            {
                req.State = RequestState.Approving;
                //repository.SaveRequest(req);
                context.SaveChanges();
                string mess = $"Заявка <b>{req.RequestID}</b> была отправлена на согласование";                
                historyService.AddRow(req, mess);
                TempData["message"] = mess;               
                await notifService.SendToApprovers(req);                
                return RedirectToAction("ListMyRequests");
            }          
        }

        public async Task<IActionResult> TestAsync()
        {
            await notifService.SendToOTRSAsync(repository.Requests.FirstOrDefault(r => r.RequestID == 6));
            return RedirectToAction("List");
        }

        /* [HttpPost]
         public async Task<IActionResult> EndApprove(Request model)
         {
             Request req = SaveRequstPost(model);
             string mess = "";
             if (req.Details.Any(d => d.ApprovingResult == ResourceApprovingResult.None))
             {
                 mess = $"Результаты согласования <b>{req.RequestID}</b> сохранены";               
             }
             else
             {
                 if (!req.Details.Any(d => d.ApprovingResult == ResourceApprovingResult.Approved))
                 {
                     req.State = RequestState.Canceled;
                     mess = $"Заявке <b>{req.RequestID}</b> было отказано в согласовании";
                 }
                 else
                 {
                     req.State = RequestState.Approved;
                     mess = $"Заявка <b>{req.RequestID}</b> согласована всеми участниками";
                     await notifService.SendToOTRSAsync(req);
                 }
                 repository.SaveRequest(req);
             }               
             historyService.AddRow(req, mess);
             TempData["message"] = mess;
             return RedirectToAction("ListToApprove");         
         }*/

        public async Task<IActionResult> EndApprove(RequestViewModel model)
        {
            Request req = SaveRequstPost(model);
            string mess = "";
            if (req.Details.Any(d => d.ApprovingResult == ResourceApprovingResult.None))
            {
                mess = $"Результаты согласования <b>{req.RequestID}</b> сохранены";
            }
            else
            {
                if (!req.Details.Any(d => d.ApprovingResult == ResourceApprovingResult.Approved))
                {
                    req.State = RequestState.Canceled;
                    mess = $"Заявке <b>{req.RequestID}</b> было отказано в согласовании";
                }
                else
                {
                    req.State = RequestState.Approved;
                    mess = $"Заявка <b>{req.RequestID}</b> согласована всеми участниками";
                    await notifService.SendToOTRSAsync(req);
                }
                context.SaveChanges();
                //repository.SaveRequest(req);
            }
            historyService.AddRow(req, mess);
            TempData["message"] = mess;
            return RedirectToAction("ListToApprove");
        }


        /*[HttpPost]
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
        }*/

        private Request SaveRequstPost(RequestViewModel model)
        {
            var details = model.Details?.Where(c => !c.IsDeleted).ToList();
           
            bool isNew = model.RequestID == 0;
            Request request = repository.Requests.FirstOrDefault(r => r.RequestID == model.RequestID);
            if (ModelState.IsValid)
            {               
                if (isNew)
                {
                    request = new Request();
                    context.Requests.Add(request);
                    request.AuthorID = ldapService.GetUserEmployee().EmployeeID;
                    request.CreateDate = DateTime.Now;
                    request.State = RequestState.New;
                }        
                
                request.Comment = model.Comment;
                var dList = new List<RequestDetail>();
                foreach (var detModel in details)
                {
                    var detail = context.RequestDetail.Find(detModel.ID);
                    if (detail == null)
                        detail = new RequestDetail();
                    detail.ApprovingResult = detModel.ApprovingResult;
                    detail.EmployeeAccessID = detModel.EmployeeAccessID;
                    detail.ResourceID = detModel.ResourceID;
                    detail.ResourceOwnerID = detModel.ResourceOwnerID;
                    detail.RoleID = detModel.RoleID;
                    detail.TicketID = detModel.TicketID;
                    dList.Add(detail);
                };
                request.Details = dList;
                request.ChangeDate = DateTime.Now;
                context.SaveChanges();
               // repository.SaveRequest(request);

                string mess = $"Заявка <b>{request.RequestID}</b> был сохранeна";
                if (isNew)
                    mess = $"Заявка <b>{request.RequestID}</b> была создана";
                //Request reqSaved = repository.Requests.First(r => r.RequestID.Equals(model.RequestID));
                historyService.AddRow(request, mess);
                TempData["message"] = mess;
                return request;
            }
            else
            {
                if (details != null)
                {
                    details.ForEach(d => d.EmployeeAccess = employeeesRepository.Employees.First(em => em.EmployeeID.Equals(d.EmployeeAccessID)));
                    details.ForEach(d => d.Resource = resourcesRepository.Resources.First(r => r.ResourceID.Equals(d.ResourceID)));
                    details.ForEach(d => d.ResourceOwner = employeeesRepository.Employees.First(em => em.EmployeeID.Equals(d.ResourceOwnerID)));
                    details.ForEach(d => d.Role = roleRepository.Roles.First(r => r.ID.Equals(d.RoleID)));
                }

                model.Details = details;
                if (request != null)
                {
                    model.Author = request.Author;
                    model.History = request.History;
                    model.State = request.State;
                }
                else
                {
                    model.Author = ldapService.GetUserEmployee();
                    model.State = RequestState.New;
                }
                // model.Author = employeeesRepository.Employees.FirstOrDefault(e => e.EmployeeID == model.AuthorID);
                return null;
            }
        }

        [HttpGet]
        public ActionResult GetRoles(string resourceId)
        {
            if (int.TryParse(resourceId, out int resId))
            {               
                IEnumerable<AccessRole> roles = resourcesRepository.Resources.First(r => r.ResourceID == int.Parse(resourceId)).Roles;
                return Json(roles);               
            }
            return Json(new EmptyResult());
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
