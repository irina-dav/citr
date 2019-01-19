using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using citr.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using citr.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

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
            Employee emplToDel = repository.GetEmployee(employeeId);
            try
            {
                repository.DeleteEmployee(employeeId);
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException.InnerException as MySql.Data.MySqlClient.MySqlException;
                if (innerException != null && innerException.Number == 1451)
                {
                    TempData["Error"] = $"Не удалось удалить сотрудника <b>{emplToDel.FullName}</b>: на него есть ссылки в других объектах";
                    return RedirectToAction(nameof(List));
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                throw;
            }
            TempData["message"] = $"Сотрудник <b>{emplToDel.FullName}</b> успешно удалён.";
            return RedirectToAction(nameof(List));
        }

        [HttpGet]
        public ActionResult GetEmployeesJson(string search)
        {
            if (search == null)
                search = "";
            var results = repository.Employees.Where(em => em.FullName.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                .Select(em => new { id = em.EmployeeID, text = em.FullName });            
            return Json(results);            
        }

       [HttpPost]
        public IActionResult LoadData()
        {
            try
            {
                var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault(); 
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                
               var employeeData = repository.Employees.Where(e => !string.IsNullOrEmpty(e.FullName)).AsQueryable();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    employeeData = employeeData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue) && searchValue.Length >= 3)
                {
                    employeeData = employeeData.Where(m => 
                        m.FullName.Contains(searchValue, StringComparison.OrdinalIgnoreCase) 
                        || m.Account.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                        || m.Email.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                        || m.Position.Contains(searchValue, StringComparison.OrdinalIgnoreCase));
                }

                recordsTotal = employeeData.Count();

                var data = employeeData.Skip(skip).Take(pageSize).ToList();

                var json = Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                return json;

            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
