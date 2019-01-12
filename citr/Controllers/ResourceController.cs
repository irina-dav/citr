using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using citr.Models;
using citr.Infrastructure;
using citr.Models.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using citr.Services;
using Microsoft.EntityFrameworkCore;

namespace citr.Controllers
{
    //[Authorize]
    public class ResourceController : Controller
    {
        private IResourceRepository repository;
        private IEmployeeRepository employeeesRepository;
        private IRequestRepository requestRepository;
        private ApplicationDbContext context;
        private readonly CategoryTree categoryTree;
        private readonly HistoryService historyService;

        public ResourceController(
            IEmployeeRepository employeeRepo, 
            IResourceRepository repo,
            IRequestRepository requestRepo,
            ApplicationDbContext ctx,
            CategoryTree catTree,
            HistoryService historySrv)
        {
            employeeesRepository = employeeRepo;
            repository = repo;
            requestRepository = requestRepo;
            context = ctx;
            categoryTree = catTree;
            historyService = historySrv;
        }

        public ViewResult List()
        {
            return View(repository.Resources);
        }

        public ViewResult Edit(int resourceId)
        {
            Resource res = repository.Resources.FirstOrDefault(p => p.ResourceID == resourceId);           
            ViewBag.Json = categoryTree.GetCategoriesJson(res.CategoryID);
            return View(res);
        }

        [HttpPost]
        public IActionResult Edit(Resource model)
        {
            var roles = model.Roles?.Where(c => !c.IsDeleted).ToList();
            if (ModelState.IsValid)
            {
                if (model.ResourceID == 0)
                {
                    model.CreationDate = DateTime.Now;
                }
                model.ChangeDate = DateTime.Now;
                model.Roles = roles;
                repository.SaveResource(model);                
                string mess = $"Ресурс {model.Name} был сохранён";

                historyService.AddRow(model, mess);
                TempData["message"] = mess;
                return RedirectToAction("List");
            }
            else
            {
                model.Roles = roles;
                return View(model);
            }
        }
       
        public ViewResult Create()
        {
            ViewBag.Json = categoryTree.GetCategoriesJson();
            return View("Edit", new Resource());
        }

        public ViewResult Copy(int sourceId)
        {            
            Resource res = repository.Resources.FirstOrDefault(p => p.ResourceID == sourceId);
            Resource newRes = new Resource()
            {
                CategoryID = res.CategoryID,
                Description = res.Description,
                Name = res.Name,
                OwnerEmployeeID = res.OwnerEmployeeID,
                Roles = new List<AccessRole>(res.Roles.Select(r => new AccessRole() { Name = r.Name }))
            };
            ViewBag.Json = categoryTree.GetCategoriesJson(res.CategoryID);         
            return View("Edit", newRes);
        }

        [HttpPost]
        public IActionResult Delete(int resourceId)
        {
            Resource deletedResource = repository.DeleteResource(resourceId);
            if (deletedResource != null)
            {
                TempData["message"] = $"{deletedResource.Name} был удалён";
            }
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult AddRole(int index, string roleName)
        {          
            var newObj = new AccessRole()
            {
                Name = roleName
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("Roles[{0}]", index);
            return PartialView("~/Views/Resource/Role.cshtml", newObj);
        }

        [HttpGet]
        public ActionResult CheckRoleReferences(int roleId)
        {
            var reqs = requestRepository.Requests.Where(r => r.Details.Any(d => d.RoleID == roleId));
            
            if (reqs.Count() > 0)
            {
                return Json(reqs.Select(r => r.RequestID));
            }
            return Json(new EmptyResult());
        }
    }
}


