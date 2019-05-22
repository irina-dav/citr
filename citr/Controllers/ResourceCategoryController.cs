using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using citr.Infrastructure;
using citr.Models;
using citr.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace citr.Controllers
{
    [Authorize(Roles="Admins")]
    public class ResourceCategoryController : Controller
    {
        private readonly CategoryTree categoryTree;
        IResourceCategoryRepository repository;

        public ResourceCategoryController(CategoryTree catTree, IResourceCategoryRepository repo)
        {
            categoryTree = catTree;
            repository = repo;
        }

        public IActionResult Index(int categoryId)
        {
            ViewBag.Json = categoryTree.GetCategoriesJson(categoryId);
            return View();
        }

        [HttpPost]
        public IActionResult AddCategory(string categoryName, string parentCategoryId)
        {
            ResourceCategory newCat = new ResourceCategory()
            {
                Name = categoryName,
                ParentCategoryID = int.Parse(parentCategoryId)
            };
            repository.SaveCategory(newCat);
            TempData["Success"] = $"Категория <strong>{newCat.Name}</strong> добавлена.";
            return RedirectToAction(nameof(Index), new { categoryId  = newCat.ID });
        }

        [HttpPost]
        public IActionResult EditCategory(string newCategoryName, int currentCategoryId)
        {
            ResourceCategory cat = repository.Categories.FirstOrDefault(c => c.ID == currentCategoryId);
            if (cat != null)
            {
                cat.Name = newCategoryName;
                repository.SaveCategory(cat);
                TempData["Success"] = $"Категория <strong>{cat.Name}</strong> изменена.";
                return RedirectToAction(nameof(Index), new { categoryId = cat.ID });
            }
            else
            {
                TempData["Success"] = $"Категория с id={currentCategoryId} не существует.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public IActionResult MoveCategory(int categoryId, int newParentId)
        {
            ResourceCategory cat = repository.Categories.FirstOrDefault(c => c.ID == categoryId);
            cat.ParentCategoryID = newParentId;
            repository.SaveCategory(cat);
            TempData["Success"] = $"Категория <strong>{cat.Name}</strong> перемещена.";
            return RedirectToAction(nameof(Index), new { categoryId = cat.ID });
        }

        [HttpPost]
        public IActionResult DeleteCategory(int categoryId)
        {
            ResourceCategory categoryToDel = repository.Categories.First(c => c.ID == categoryId);
            try
            {
                repository.DeleteCategory(categoryId);
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException.InnerException as MySql.Data.MySqlClient.MySqlException;
                if (innerException != null && innerException.Number == 1451)
                {
                    TempData["Error"] = $"Не удалось удалить категорию <strong>{categoryToDel.Name}</strong>: на неё есть ссылки в других объектах.";
                    return RedirectToAction(nameof(Index));
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
            TempData["Success"] = $"Категория <strong>{categoryToDel.Name}</strong> успешно удалена.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public ActionResult GetCategoriesJson(string search)
        {
            if (search == null)
                search = "";
            var results = repository.Categories
                .Where(с => с.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(c => c.Name).Select(c => new { id = c.ID, text = c.Name });
            return Json(results);
        }
    }
}
