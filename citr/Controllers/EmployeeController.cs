﻿using citr.Models;
using citr.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace citr.Controllers
{
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
            return Json(!repository.Employees.Any(x => x.Account.Equals(Account, comparisonType: StringComparison.InvariantCultureIgnoreCase) && x.EmployeeID != EmployeeID));
        }

        [Authorize(Roles = "Admins")]
        public ViewResult List()
        {
            return View(repository.Employees);
        }

        [Authorize(Roles = "Admins")]
        public ViewResult Edit(int employeeId)
        {
            Employee empl = repository.Employees.FirstOrDefault(p => p.EmployeeID == employeeId);
            IEnumerable<SelectListItem> items = db.UserRole.Select(r => new SelectListItem
            {
                Value = r.ID.ToString(),
                Text = r.Name

            });
            EmployeeViewModel model = new EmployeeViewModel(empl)
            {
                Roles = items
            };
            return View(model);
        }

        [Authorize(Roles = "Admins")]
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

        [Authorize(Roles = "Admins")]
        public ViewResult Create()
        {
            return View("Edit", new EmployeeViewModel());
        }

        [Authorize(Roles = "Admins")]
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
                MySql.Data.MySqlClient.MySqlException innerException = ex.InnerException.InnerException as MySql.Data.MySqlClient.MySqlException;
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

        [Authorize]
        [HttpGet]
        public ActionResult GetEmployeesJson(string search)
        {
            if (search == null)
            {
                search = "";
            }

            var results = repository.Employees.Where(em => em.FullName.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(em => em.FullName)
                .Select(em => new { id = em.EmployeeID, text = em.FullName });
            return Json(results);
        }

        [Authorize]
        [HttpPost]
        public IActionResult LoadData()
        {
            try
            {
                string draw = HttpContext.Request.Form["draw"].FirstOrDefault();
                string start = Request.Form["start"].FirstOrDefault();
                string length = Request.Form["length"].FirstOrDefault();
                string sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                string sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                string searchValue = Request.Form["search[value]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                IQueryable<Employee> employeeData = repository.Employees.Where(e => !string.IsNullOrEmpty(e.FullName)).AsQueryable();

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

                List<Employee> data = employeeData.Skip(skip).Take(pageSize).ToList();

                JsonResult json = Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                return json;

            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
