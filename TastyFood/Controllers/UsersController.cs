using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TastyFood.Data;
using TastyFood.Models;

namespace TastyFood.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var loggedInUser = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var usersFromDB = await _db.Users
                .Where(p => p.Id != loggedInUser.Value).ToListAsync();

            return View(usersFromDB);
        }

        // GET Edit Users
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await _db.ApplicationUser.SingleOrDefaultAsync(p => p.Id == id);
            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }

        // POST Edit Users
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser appUser)
        {
            if (id != appUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userFromDB = await _db.Users.Where(p => p.Id == id).FirstOrDefaultAsync();
                if (userFromDB == null)
                {
                    return NotFound();
                }

                userFromDB.FirstName = appUser.FirstName;
                userFromDB.LastName = appUser.LastName;
                userFromDB.PhoneNumber = appUser.PhoneNumber;
                userFromDB.LockoutEnd = appUser.LockoutEnd;
                userFromDB.LockoutEnabled = appUser.LockoutEnabled;
                userFromDB.LockoutReason = appUser.LockoutReason;
                userFromDB.AccessFailedCount = appUser.AccessFailedCount;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(appUser);
        }

        // GET Lock Users
        public async Task<IActionResult> Lock(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await _db.ApplicationUser.SingleOrDefaultAsync(p => p.Id == id);
            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }

        // POST Lock Users
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id, ApplicationUser appUser)
        {
            if (id != appUser.Id)
            {
                return NotFound();
            }

            var userFromDB = await _db.Users.Where(p => p.Id == id).FirstOrDefaultAsync();
            if (userFromDB == null)
            {
                return NotFound();
            }
            
            userFromDB.LockoutEnd = DateTime.Now.AddYears(100);
            userFromDB.LockoutEnabled = true;
            userFromDB.LockoutReason = appUser.LockoutReason;
            userFromDB.AccessFailedCount = appUser.AccessFailedCount;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET UnLock Users
        public async Task<IActionResult> UnLock(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await _db.ApplicationUser.SingleOrDefaultAsync(p => p.Id == id);
            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }

        // POST UnLock Users
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnLock(string id, ApplicationUser appUser)
        {
            if (id != appUser.Id)
            {
                return NotFound();
            }

            var userFromDB = await _db.Users.Where(p => p.Id == id).FirstOrDefaultAsync();
            if (userFromDB == null)
            {
                return NotFound();
            }
            
            userFromDB.LockoutEnd = null;
            userFromDB.LockoutEnabled = appUser.LockoutEnabled;
            userFromDB.LockoutReason = appUser.LockoutReason;
            userFromDB.UnLockReason = appUser.UnLockReason;
            userFromDB.AccessFailedCount = appUser.AccessFailedCount;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}