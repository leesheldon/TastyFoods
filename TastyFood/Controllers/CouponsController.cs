using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TastyFood.Data;
using TastyFood.Models;

namespace TastyFood.Controllers
{
    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.Coupon.ToListAsync());
        }

        // GET Coupons
        public IActionResult Create()
        {
            return View();
        }

        // POST Create Coupons
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files[0] != null && files[0].Length > 0)
                {
                    byte[] bPicture = null;
                    using (var fs = files[0].OpenReadStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);
                            bPicture = ms.ToArray();
                        }
                    }

                    coupon.Picture = bPicture;
                }

                _db.Coupon.Add(coupon);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(coupon);
        }

        // GET Edit Coupons
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);
            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        // POST Edit Coupons
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon coupon)
        {
            if (id != coupon.Id)
            {
                return NotFound();
            }

            var couponFromDB = await _db.Coupon.Where(p => p.Id == id).FirstOrDefaultAsync();
            if (couponFromDB == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files[0] != null && files[0].Length > 0)
                {
                    byte[] bPicture = null;
                    using (var fs = files[0].OpenReadStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);
                            bPicture = ms.ToArray();
                        }
                    }

                    couponFromDB.Picture = bPicture;
                }

                couponFromDB.Name = coupon.Name;
                couponFromDB.MinimumAmount = coupon.MinimumAmount;
                couponFromDB.isActive = coupon.isActive;
                couponFromDB.Discount = coupon.Discount;
                couponFromDB.CouponType = coupon.CouponType;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(coupon);
        }

        // GET Delete Coupons
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);
            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        // POST Delete MenuItems
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var couponFromDB = await _db.Coupon.SingleOrDefaultAsync(p => p.Id == id);
            if (couponFromDB == null)
            {
                return NotFound();
            }

            _db.Coupon.Remove(couponFromDB);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}