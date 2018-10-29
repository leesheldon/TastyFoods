using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TastyFood.Data;
using TastyFood.Models;
using TastyFood.Models.UsersViewModels;
using TastyFood.Utility;

namespace TastyFood.Controllers
{
    //[Authorize(Roles = SD.AdminEndUser)]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        UserViewModel _userVM = new UserViewModel
        {

        };

        public UsersController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var loggedInUser = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (loggedInUser == null)
            {
                return NotFound();
            }

            var usersFromDB = await _db.Users
                .Where(p => p.Id != loggedInUser.Value).ToListAsync();

            foreach (var perUser in usersFromDB)
            {
                perUser.IsLockedOut = await CheckUserIsLockedOut(perUser);
                perUser.RolesNames = await GetRolesNameListBySelectedUser(perUser);
            }
           
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

            // Check selected user is locked out or not? And Get roles name list
            appUser.IsLockedOut = await CheckUserIsLockedOut(appUser);
            appUser.RolesNames = await GetRolesNameListBySelectedUser(appUser);                       
            
            _userVM.SelectedUser = appUser;

            GetRolesListBySelectedUser(id);

            return View(_userVM);
        }
                
        // POST Edit Users
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserViewModel userVM)
        {
            ApplicationUser appUser = userVM.SelectedUser;
            List<RolesListOfSelectedUser> appRoles = userVM.RolesList;

            if (id != appUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (appRoles.Count < 1)
                {
                    return NotFound();
                }
                
                var userFromDB = await _db.Users.Where(p => p.Id == id).FirstOrDefaultAsync();
                if (userFromDB == null)
                {
                    return NotFound();
                }

                userFromDB.FirstName = appUser.FirstName;
                userFromDB.LastName = appUser.LastName;
                userFromDB.PhoneNumber = appUser.PhoneNumber;
                userFromDB.LockoutReason = appUser.LockoutReason;
                userFromDB.AccessFailedCount = appUser.AccessFailedCount;

                if (appUser.LockoutEnd < DateTime.Now)
                {
                    userFromDB.LockoutEnabled = false;
                }

                userFromDB.LockoutEnd = appUser.LockoutEnd;

                // Update roles list                
                List<string> new_Roles = new List<string>();

                foreach (var itemRole in appRoles)
                {
                    if (itemRole.SelectedRole)
                    {
                        new_Roles.Add(itemRole.Name);
                    }
                }

                if (new_Roles.Count < 1)
                {
                    return BadRequest("Please select at least one role.");
                }

                var old_Roles = await _userManager.GetRolesAsync(appUser);

                var result = await _userManager.RemoveFromRolesAsync(userFromDB, old_Roles);
                if (!result.Succeeded)
                    BadRequest("Failed to remove old roles.");

                result = await _userManager.AddToRolesAsync(userFromDB, new_Roles);
                if (!result.Succeeded)
                    BadRequest("Failed to add new roles.");


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
            userFromDB.IsLockedOut = true;

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
            userFromDB.LockoutEnabled = false;
            userFromDB.LockoutReason = appUser.LockoutReason;
            userFromDB.UnLockReason = appUser.UnLockReason;
            userFromDB.AccessFailedCount = appUser.AccessFailedCount;
            userFromDB.IsLockedOut = false;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GetRolesNameListBySelectedUser(ApplicationUser appUser)
        {
            var rolesNameList = await _userManager.GetRolesAsync(appUser);
            var strRolesList = "";

            foreach (var roleItem in rolesNameList)
            {
                strRolesList = strRolesList + roleItem + ", ";
            }

            if (!string.IsNullOrEmpty(strRolesList))
            {
                strRolesList = strRolesList.Substring(0, strRolesList.Length - 2);
            }

            return strRolesList;
        }

        private async Task<bool> CheckUserIsLockedOut(ApplicationUser appUser)
        {
            var isLockedOut = await _userManager.IsLockedOutAsync(appUser);

            return isLockedOut;
        }

        private async void GetRolesListBySelectedUser(string id)
        {
            // Get selected roles list checked by selected user
            var selectedRolesIds = await _db.UserRoles.Where(p => p.UserId == id).Select(p => p.RoleId).ToListAsync();

            // Get Roles list from Database
            var rolesFromDB = await _db.Roles.Distinct().ToListAsync();

            // Return selected Roles within Roles list from Database
            List<RolesListOfSelectedUser> appRoles = new List<RolesListOfSelectedUser>();
            foreach (var itemRole in rolesFromDB)
            {
                RolesListOfSelectedUser newRole = new RolesListOfSelectedUser();
                newRole.Id = itemRole.Id;
                newRole.Name = itemRole.Name;
                newRole.SelectedRole = false;

                if (selectedRolesIds.Exists(str => str.Equals(itemRole.Id)))
                {
                    newRole.SelectedRole = true;
                }

                appRoles.Add(newRole);
            }

            _userVM.RolesList = appRoles;            
        }

    }
}