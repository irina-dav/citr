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
    public class ResourceController : Controller
    {
        private readonly IResourceRepository repository;
        private readonly IEmployeeRepository employeeesRepository;
        private readonly IRequestRepository requestRepository;
        private readonly ApplicationDbContext context;
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

        [Authorize(Roles = "Admins")]
        public ViewResult List()
        {
            return View(repository.Resources);
        }

        [Authorize(Roles = "Admins")]
        public ViewResult Edit(int resourceId)
        {
            Resource res = repository.Resources.FirstOrDefault(p => p.ResourceID == resourceId);           
            ViewBag.Json = categoryTree.GetCategoriesJson(res.CategoryID);
            return View(res);
        }

        [Authorize(Roles = "Admins")]
        [HttpPost]
        public IActionResult Edit(Resource model)
        {
            var roles = model.Roles?.Where(c => !c.IsDeleted).ToList();
            if (ModelState.IsValid)
            {
                bool isNew = model.ResourceID == 0;
                if (isNew)
                {
                    model.CreationDate = DateTime.Now;
                }
                model.ChangeDate = DateTime.Now;
                model.Roles = roles;
                repository.SaveResource(model);
                Resource res = repository.Resources.First(r => r.ResourceID.Equals(model.ResourceID));

                string mess = $"Ресурс <b>{res.Name}</b> был сохранён";
                if (isNew)
                    mess = $"Ресурс <b>{res.Name}</b> был создан";

                historyService.AddRow(res, mess);
                TempData["message"] = mess;
                return RedirectToAction("List");
            }
            else
            {
                model.Roles = roles;
                return View(model);
            }
        }

        [Authorize(Roles = "Admins")]
        public ViewResult Create()
        {
            ViewBag.Json = categoryTree.GetCategoriesJson();
            return View("Edit", new Resource());
        }

        [Authorize(Roles = "Admins")]
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

        [Authorize(Roles = "Admins")]
        [HttpPost]
        public IActionResult Delete(int resourceId)
        {
            Resource deletedResource = repository.DeleteResource(resourceId);
            if (deletedResource != null)
            {
                TempData["message"] = $"<b>{deletedResource.Name}</b> был удалён";
            }
            return RedirectToAction("List");
        }

        [Authorize(Roles = "Admins")]
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

        [Authorize]
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


        [Authorize]
        [HttpGet]
        public ActionResult GetResourcesJson(string search)
        {
            if (search == null)
                search = "";
            JsonResult res = Json(new EmptyResult());
            List<object> results = new List<object>();
            foreach (var g in repository.Resources.Where(r => r.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)).GroupBy(r => r.Category))
            {
                results.Add(new { text = g.Key.Name, children = g.ToList().Select(r => new { id = r.ResourceID, text = r.Name }) });
            }
            return Json(results);
        }
    }
}


