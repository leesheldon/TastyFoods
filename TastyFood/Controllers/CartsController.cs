using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TastyFood.Data;
using TastyFood.Models;
using TastyFood.Models.CartDetailsViewModels;
using TastyFood.Utility;

namespace TastyFood.Controllers
{
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public CartDetailsViewModel detailsCartVM { get; set; }

        public CartsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            detailsCartVM = new CartDetailsViewModel()
            {
                OrderHeader = new OrderHeader()
            };

            detailsCartVM.OrderHeader.OrderTotal = 0;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var carts = _db.ShoppingCart.Where(p => p.ApplicationUserId == claim.Value);
            if (carts == null)
            {
                return NotFound();
            }

            detailsCartVM.listCart = carts.ToList();
            foreach (var perCart in detailsCartVM.listCart)
            {
                perCart.MenuItem = _db.MenuItem.FirstOrDefault(p => p.Id == perCart.MenuItemId);
                detailsCartVM.OrderHeader.OrderTotal = detailsCartVM.OrderHeader.OrderTotal + (perCart.MenuItem.Price * perCart.Count);

                if (perCart.MenuItem.Description.Length > 100)
                {
                    perCart.MenuItem.Description = perCart.MenuItem.Description.Substring(0, 99) + "...";
                }
            }

            detailsCartVM.OrderHeader.PickupTime = DateTime.Now;

            return View(detailsCartVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            detailsCartVM.listCart = _db.ShoppingCart
                    .Where(p => p.ApplicationUserId == claim.Value)
                    .ToList();

            // Order Header
            detailsCartVM.OrderHeader.OrderDate = DateTime.Now;
            detailsCartVM.OrderHeader.UserId = claim.Value;
            detailsCartVM.OrderHeader.Status = SD.StatusSubmitted;
            OrderHeader orderHeader = detailsCartVM.OrderHeader;

            _db.OrderHeader.Add(orderHeader);
            _db.SaveChanges();

            // Order Details
            foreach(var cart in detailsCartVM.listCart)
            {
                cart.MenuItem = _db.MenuItem.FirstOrDefault(p => p.Id == cart.MenuItemId);
                if (cart.MenuItem != null)
                {
                    OrderDetails orderDetails = new OrderDetails
                    {
                        MenuItemId = cart.MenuItemId,
                        OrderId = orderHeader.Id,
                        Description = cart.MenuItem.Description,
                        Name = cart.MenuItem.Name,
                        Price = cart.MenuItem.Price,
                        Count = cart.Count
                    };

                    _db.OrderDetails.Add(orderDetails);
                }
            }

            // Remove shopping carts
            _db.ShoppingCart.RemoveRange(detailsCartVM.listCart);

            _db.SaveChanges();

            // Restore Session to zero
            HttpContext.Session.SetInt32(SD.SessionCountCarts, 0);

            return RedirectToAction("Confirm", "Orders", new { orderHeaderId = orderHeader.Id });
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _db.ShoppingCart.Where(p => p.Id == cartId).FirstOrDefault();
            cart.Count += 1;
            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _db.ShoppingCart.Where(p => p.Id == cartId).FirstOrDefault();
            if (cart.Count == 1)
            {
                _db.ShoppingCart.Remove(cart);
                _db.SaveChanges();

                var cnt = _db.ShoppingCart
                    .Where(p => p.ApplicationUserId == cart.ApplicationUserId)
                    .ToList()
                    .Count();

                HttpContext.Session.SetInt32(SD.SessionCountCarts, cnt);
            }
            else
            {
                cart.Count -= 1;
                _db.SaveChanges();
            }
                        
            return RedirectToAction(nameof(Index));
        }


    }
}