using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TastyFood.Data;
using TastyFood.Models;
using TastyFood.Models.SubCategoryViewModels;

namespace TastyFood.Controllers
{
    public class SubCategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        [TempData]
        public string StatusMessage { get; set; }

        public SubCategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET Action
        public async Task<IActionResult> Index()
        {
            var subCategories = _db.SubCategory.Include(p => p.Category);

            return View(await subCategories.ToListAsync());
        }

        // GET Action for Create
        public IActionResult Create()
        {
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel
            {
                CategoryList = _db.Category.ToList(),
                SubCategory = new SubCategory(),
                SubCategoryList = _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToList()
            };

            return View(model);
        }

        // POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Where(p => p.Name == model.SubCategory.Name).Count();
                var doesSubCatAndCatExists = _db.SubCategory.Where(p => p.Name == model.SubCategory.Name && p.CategoryId == model.SubCategory.CategoryId).Count();

                if (doesSubCategoryExists > 0 && model.isNew)
                {
                    // error
                    StatusMessage = "Error: Sub Category Name already exists.";
                }
                else
                {
                    if (doesSubCategoryExists == 0 && !model.isNew)
                    {
                        // error
                        StatusMessage = "Error: Sub Category does NOT exist.";
                    }
                    else
                    {
                        if (doesSubCatAndCatExists > 0)
                        {
                            // error
                            StatusMessage = "Error: Category and Sub Category already exists.";
                        }
                        else
                        {
                            _db.Add(model.SubCategory);
                            await _db.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }

            SubCategoryAndCategoryViewModel modelToView = new SubCategoryAndCategoryViewModel
            {
                CategoryList = _db.Category.ToList(),
                SubCategory = new SubCategory(),
                SubCategoryList = _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).ToList(),
                StatusMessage = StatusMessage
            };

            return View(modelToView);
        }

        // GET Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(p => p.Id == id);
            if (subCategory == null) return NotFound();

            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel
            {
                CategoryList = _db.Category.ToList(),
                SubCategory = subCategory,
                SubCategoryList = _db.SubCategory.Select(p => p.Name).Distinct().ToList()
            };

            return View(model);
        }

        // POST Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Where(p => p.Name == model.SubCategory.Name).Count();
                var doesSubCatAndCatExists = _db.SubCategory.Where(p => p.Name == model.SubCategory.Name && p.CategoryId == model.SubCategory.CategoryId).Count();

                if (doesSubCategoryExists == 0)
                {
                    StatusMessage = "Error: Sub Category does not exist. You cannot add a new sub category here.";
                }
                else
                {
                    if (doesSubCatAndCatExists > 0)
                    {
                        StatusMessage = "Error: Category and Sub Category already exists.";
                    }
                    else
                    {
                        var subCatFromDB = _db.SubCategory.Find(id);
                        subCatFromDB.Name = model.SubCategory.Name;
                        subCatFromDB.CategoryId = model.SubCategory.CategoryId;

                        await _db.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            SubCategoryAndCategoryViewModel modelToView = new SubCategoryAndCategoryViewModel
            {
                CategoryList = _db.Category.ToList(),
                SubCategory = model.SubCategory,
                SubCategoryList = _db.SubCategory.Select(p => p.Name).Distinct().ToList(),
                StatusMessage = StatusMessage
            };

            return View(modelToView);
        }

        // GET Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var subCategory = await _db.SubCategory.Include(p => p.Category).SingleOrDefaultAsync(p => p.Id == id);
            if (subCategory == null) return NotFound();
                        
            return View(subCategory);
        }

        // GET Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var subCategory = await _db.SubCategory.Include(p => p.Category).SingleOrDefaultAsync(p => p.Id == id);
            if (subCategory == null) return NotFound();

            return View(subCategory);
        }

        // POST Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subCategoryFromDB = await _db.SubCategory.SingleOrDefaultAsync(p => p.Id == id);
            _db.SubCategory.Remove(subCategoryFromDB);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}