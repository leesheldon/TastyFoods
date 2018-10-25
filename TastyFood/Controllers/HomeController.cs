using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TastyFood.Data;
using TastyFood.Models;
using TastyFood.Models.HomeViewModels;

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

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
