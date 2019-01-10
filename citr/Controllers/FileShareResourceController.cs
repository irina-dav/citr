using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequestsAccess.Models;
using RequestsAccess.Models.ViewModels;

namespace RequestsAccess.Controllers
{/*
    [Authorize]
    public class FileShareResourceController : Controller
    {
        private IFileShareResourceRepository repository;
        private IEmployeeRepository employeeesRepository;

        public FileShareResourceController(IEmployeeRepository employeeRepo, IFileShareResourceRepository repo)
        {
            employeeesRepository = employeeRepo;
            repository = repo;
        }

        public ViewResult List()
        {
            return View(repository.Resources);
        }

        public ViewResult Edit(int resourceId)
        {
            FileShareResource res = repository.Resources.FirstOrDefault(p => p.ResourceID.Equals(resourceId));
            return View(res);
        }

        [HttpPost]
        public IActionResult Edit(FileShareResource model)
        {
            if (ModelState.IsValid)
            {               
                model.ChangeDate = DateTime.Now;
                repository.SaveResource(model);
                TempData["message"] = $"Ресурс {model.ResourceID} был сохранён";
                return RedirectToAction("List");
            }
            else
            {
                return View(model);
            }
        }

        public ViewResult Create()
        {
            var viewModel = new FileShareResource();
            return View("Edit", viewModel);
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
        
    }*/
}