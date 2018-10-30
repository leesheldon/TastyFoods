using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TastyFood.Data;
using TastyFood.Models;
using TastyFood.Models.HomeViewModels;
using TastyFood.Utility;

namespace TastyFood.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            IndexViewModel IndexVM = new IndexViewModel
            {
                MenuItem = await _db.MenuItem
                    .Include(p => p.Category)
                    .Include(p => p.SubCategory)
                    .ToListAsync(),
                Category = _db.Category.OrderBy(p => p.DisplayOrder),
                Coupons = _db.Coupon
                    .Where(p => p.isActive == true).ToList()
            };

            return View(IndexVM);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var menuItemFromDB = await _db.MenuItem
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();

            ShoppingCart cart = new ShoppingCart()
            {
                MenuItem = menuItemFromDB,
                MenuItemId = menuItemFromDB.Id
            };

            return View(cart);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart cart)
        {
            cart.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                cart.ApplicationUserId = claim.Value;
                ShoppingCart cartFromDB = await _db.ShoppingCart
                    .Where(p => p.ApplicationUserId == cart.ApplicationUserId && p.MenuItemId == cart.MenuItemId)
                    .FirstOrDefaultAsync();

                if (cartFromDB == null)
                {
                    // This menu item does not exist
                    _db.ShoppingCart.Add(cart);
                }
                else
                {
                    // This menu item exists in shopping cart for this user, so just update the count value.
                    cartFromDB.Count = cartFromDB.Count + cart.Count;
                }

                await _db.SaveChangesAsync();

                var numberOfCarts = _db.ShoppingCart
                        .Where(p => p.ApplicationUserId == cart.ApplicationUserId)
                        .ToList()
                        .Count;

                // Set Cart's Session
                HttpContext.Session.SetInt32(SD.SessionCountCarts, numberOfCarts);


                return RedirectToAction(nameof(Index));
            }
            else
            {
                var menuItemFromDB = await _db.MenuItem
                    .Include(m => m.Category)
                    .Include(m => m.SubCategory)
                    .Where(m => m.Id == cart.MenuItemId)
                    .FirstOrDefaultAsync();

                ShoppingCart cartToReturn = new ShoppingCart()
                {
                    MenuItem = menuItemFromDB,
                    MenuItemId = menuItemFromDB.Id
                };

                return View(cartToReturn);
            }

        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
