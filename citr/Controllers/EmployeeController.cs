using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using citr.Models;

namespace citr.Controllers
{
    public class EmployeeController : Controller
    {
        private IEmployeeRepository repository;

        public EmployeeController(IEmployeeRepository repo)
        {
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
            return View(empl);
        }

        [HttpPost]
        public IActionResult Edit(Employee model)
        {
            if (ModelState.IsValid)
            {
                string mess = $"Сотрудник {model.FullName} был сохранён";
                repository.SaveEmployee(model);
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
            return View("Edit", new Employee());
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
    }
}