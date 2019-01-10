using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequestsAccess.Models;
using RequestsAccess.Infrastructure;
using RequestsAccess.Models.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RequestsAccess.Services;

namespace RequestsAccess.Controllers
{
    //[Authorize]
    public class ResourceController : Controller
    {
        private IResourceRepository repository;
        private IEmployeeRepository employeeesRepository;
        private ApplicationDbContext context;
        private readonly CategoryTree categoryTree;
        private readonly HistoryService historyService;

        public ResourceController(IEmployeeRepository employeeRepo, IResourceRepository repo, ApplicationDbContext ctx, CategoryTree catTree, HistoryService historySrv)
        {
            employeeesRepository = employeeRepo;
            repository = repo;
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
            if (ModelState.IsValid)
            {
                if (model.ResourceID == 0)
                {
                    model.CreationDate = DateTime.Now;
                }
                model.ChangeDate = DateTime.Now;
                string mess = $"Ресурс {model.Name} был сохранён";               
                repository.SaveResource(model);
                historyService.AddRow(model, mess);
                TempData["message"] = mess;
                return RedirectToAction("List");
            }
            else
            {
                return View(model);
            }
        }
       
        public ViewResult Create()
        {
            ViewBag.Json = categoryTree.GetCategoriesJson();
            return View("Edit", new Resource());
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
    }
}


