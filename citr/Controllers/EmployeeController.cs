using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using citr.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using citr.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace citr.Controllers
{
    [Authorize(Roles = "Admins")]
    public class EmployeeController : Controller
    {
        private IEmployeeRepository repository;
        private ApplicationDbContext db;

        public EmployeeController(IEmployeeRepository repo, ApplicationDbContext db)
        {
            this.db = db;
            repository = repo;
        }

        public JsonResult IsAccountExist(string Account, int EmployeeID)
        {            
            return Json(!repository.Employees.Any( x => x.Account.Equals(Account, comparisonType: StringComparison.InvariantCultureIgnoreCase) && x.EmployeeID != EmployeeID));
        }

        public ViewResult List()
        {
            return View(repository.Employees);
        }

        public ViewResult Edit(int employeeId)
        {
            Employee empl = repository.Employees.FirstOrDefault(p => p.EmployeeID == employeeId);
            IEnumerable<SelectListItem> items = db.UserRole.Select(r => new SelectListItem
            {
                Value = r.ID.ToString(),
                Text = r.Name

            });
            EmployeeViewModel model = new EmployeeViewModel(empl);
            model.Roles = items;
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {

                Employee empl;
                if (model.EmployeeID == 0)
                {
                    empl = new Employee();
                }
                else
                {
                    empl = repository.Employees.First(p => p.EmployeeID == model.EmployeeID);
                }
                empl.Account = model.Account;
                empl.Email = model.Email;
                empl.FullName = model.FullName;
                empl.Position = model.Position;
                empl.UserRoleID = model.UserRoleID;
                repository.SaveEmployee(empl);
                string mess = $"Сотрудник <b>{model.FullName}</b> был сохранён";
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
            return View("Edit", new EmployeeViewModel());
        }

        [HttpPost]
        public IActionResult Delete(int employeeId)
        {
            Employee deletedEmployee = repository.DeleteEmployee(employeeId);
            if (deletedEmployee != null)
            {
                TempData["message"] = $"Сотрудников {deletedEmployee.FullName} был удалён";
            }
            return RedirectToAction("List");
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetEmployeesJson(string search)
        {
            if (search == null)
                search = "";
            var results = repository.Employees.Where(em => em.FullName.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                .Select(em => new { id = em.EmployeeID, text = em.FullName });            
            return Json(results);            
        }
    }
}