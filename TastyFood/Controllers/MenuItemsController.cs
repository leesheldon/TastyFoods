using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TastyFood.Data;
using TastyFood.Models;
using TastyFood.Models.MenuItemViewModels;
using TastyFood.Utility;

namespace TastyFood.Controllers
{
    [Authorize(Roles = SD.AdminEndUser)]
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        public MenuItemsController(ApplicationDbContext db, IHostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            MenuItemVM = new MenuItemViewModel
            {
                Category = _db.Category.ToList(),
                MenuItem = new Models.MenuItem()
            };
        }

        // GET MenuItems
        public async Task<IActionResult> Index()
        {
            var menuItems = _db.MenuItem.Include(m => m.SubCategory).Include(m => m.Category);

            return View(await menuItems.ToListAsync());
        }

        // GET Create MenuItems
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }
        
        // POST Create MenuItems
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewItem()
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            _db.MenuItem.Add(MenuItemVM.MenuItem);
            await _db.SaveChangesAsync();

            // Saving Image
            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDB = _db.MenuItem.Find(MenuItemVM.MenuItem.Id);
            if (files[0] != null && files[0].Length > 0)
            {
                // When user upload an image
                var extensionOfFile = files[0].FileName.Substring(files[0].FileName.LastIndexOf("."),
                    files[0].FileName.Length - files[0].FileName.LastIndexOf("."));

                string imageFileName = MenuItemVM.MenuItem.Id + extensionOfFile;
                string imageFilePath = @"images\" + imageFileName;
                string uploadLocation = Path.Combine(webRootPath, imageFilePath);

                using (var fileStream = new FileStream(uploadLocation, FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }

                menuItemFromDB.Image = @"\" + imageFilePath;
            }
            else
            {
                // When user does NOT upload an image
                string defaultImageFilePath = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                string newDefaultImageFilePath = @"images\" + MenuItemVM.MenuItem.Id + ".png";
                string uploadLocation = Path.Combine(webRootPath, newDefaultImageFilePath);

                System.IO.File.Copy(defaultImageFilePath, uploadLocation);
                menuItemFromDB.Image = @"\" + newDefaultImageFilePath;
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public JsonResult GetSubCategories(int categoryId)
        {
            List<SubCategory> subCategoriesList = new List<SubCategory>();

            subCategoriesList = (from subCategory in _db.SubCategory
                             where subCategory.CategoryId == categoryId
                             select subCategory).ToList();

            return Json(new SelectList(subCategoriesList, "Id", "Name"));
        }

        // GET Edit MenuItems
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItem
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .SingleOrDefaultAsync(m => m.Id == id);

            MenuItemVM.SubCategory = _db.SubCategory
                .Where(m => m.CategoryId == MenuItemVM.MenuItem.CategoryId)
                .ToList();

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        // POST Edit MenuItems
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (id != MenuItemVM.MenuItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    var files = HttpContext.Request.Form.Files;
                    var menuItemFromDB = _db.MenuItem
                        .Where(m => m.Id == MenuItemVM.MenuItem.Id)
                        .FirstOrDefault();

                    if (files[0]!=null && files[0].Length>0)
                    {
                        var newExtensionOfFile = files[0].FileName.Substring(files[0].FileName.LastIndexOf("."),
                            files[0].FileName.Length - files[0].FileName.LastIndexOf("."));
                        var oldExtensionOfFile = menuItemFromDB.Image.Substring(menuItemFromDB.Image.LastIndexOf("."),
                            menuItemFromDB.Image.Length - menuItemFromDB.Image.LastIndexOf("."));

                        string newImageFileName = MenuItemVM.MenuItem.Id + newExtensionOfFile;
                        string oldImageFileName = MenuItemVM.MenuItem.Id + oldExtensionOfFile;

                        string newImageFilePath = @"images\" + newImageFileName;
                        string oldImageFilePath = @"images\" + oldImageFileName;

                        string uploadLocation = Path.Combine(webRootPath, oldImageFilePath);

                        if (System.IO.File.Exists(uploadLocation))
                        {
                            System.IO.File.Delete(uploadLocation);
                        }

                        uploadLocation = Path.Combine(webRootPath, newImageFilePath);
                        using (var fileStream = new FileStream(uploadLocation, FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }

                        MenuItemVM.MenuItem.Image = @"\" + newImageFilePath;
                    }
                    
                    if (MenuItemVM.MenuItem.Image !=null)
                    {
                        menuItemFromDB.Image = MenuItemVM.MenuItem.Image;
                    }

                    menuItemFromDB.Name = MenuItemVM.MenuItem.Name;
                    menuItemFromDB.Description = MenuItemVM.MenuItem.Description;
                    menuItemFromDB.Price = MenuItemVM.MenuItem.Price;
                    menuItemFromDB.Spicyness = MenuItemVM.MenuItem.Spicyness;
                    menuItemFromDB.CategoryId = MenuItemVM.MenuItem.CategoryId;
                    menuItemFromDB.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;

                    await _db.SaveChangesAsync();
                }
                catch(Exception ex)
                {

                }

                return RedirectToAction(nameof(Index));
            }

            MenuItemVM.SubCategory = _db.SubCategory
                .Where(m => m.CategoryId == MenuItemVM.MenuItem.CategoryId)
                .ToList();
            return View(MenuItemVM);
        }

        // GET Details MenuItems
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItem
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .SingleOrDefaultAsync(m => m.Id == id);           

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(MenuItemVM.MenuItem.Spicyness))
            {
                int enumIdx = int.Parse(MenuItemVM.MenuItem.Spicyness);
                MenuItemVM.SelectedESpicyText = Enum.GetName(typeof(MenuItem.ESpicy), enumIdx);
            }           

            return View(MenuItemVM);
        }

        // GET Delete MenuItems
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItem
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(MenuItemVM.MenuItem.Spicyness))
            {
                int enumIdx = int.Parse(MenuItemVM.MenuItem.Spicyness);
                MenuItemVM.SelectedESpicyText = Enum.GetName(typeof(MenuItem.ESpicy), enumIdx);
            }

            return View(MenuItemVM);
        }

        // POST Delete MenuItems
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            MenuItem menuItem = await _db.MenuItem.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }

            string webRootPath = _hostingEnvironment.WebRootPath;
            var extensionOfFile = menuItem.Image.Substring(menuItem.Image.LastIndexOf("."),
                    menuItem.Image.Length - menuItem.Image.LastIndexOf("."));
            string imageFileName = menuItem.Id + extensionOfFile;
            string imageFilePath = @"images\" + imageFileName;
            string uploadLocation = Path.Combine(webRootPath, imageFilePath);

            if (System.IO.File.Exists(uploadLocation))
            {
                System.IO.File.Delete(uploadLocation);
            }

            _db.MenuItem.Remove(menuItem);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}