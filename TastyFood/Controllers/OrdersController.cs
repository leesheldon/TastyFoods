using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TastyFood.Data;
using TastyFood.Models;
using TastyFood.Models.OrderDetailsViewModels;
using TastyFood.Models.OrderExportViewModels;
using TastyFood.Models.OrderListViewModels;
using TastyFood.Utility;

namespace TastyFood.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private int PageSize = 2;

        public OrdersController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Confirm GET        
        public async Task<IActionResult> Confirm(string orderHeaderId)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewModel orderDetailsVM = new OrderDetailsViewModel
            {
                OrderHeader = _db.OrderHeader
                        .Where(p => p.Id == orderHeaderId && p.UserId == claim.Value)
                        .FirstOrDefault(),
                OrderDetails = _db.OrderDetails
                        .Where(p => p.OrderId == orderHeaderId)
                        .ToList()
            };

            return View(orderDetailsVM);
        }
        
        public IActionResult OrderHistory(int productPage = 1)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            OrderListViewModel orderListVM = new OrderListViewModel
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            List<OrderHeader> orderHeadersList = _db.OrderHeader
                    .Where(p => p.UserId == claim.Value)
                    .OrderByDescending(p => p.OrderDate)
                    .ToList();

            foreach (OrderHeader item in orderHeadersList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel();
                individual.OrderHeader = item;
                individual.OrderDetails = _db.OrderDetails
                        .Where(p => p.OrderId == item.Id)
                        .ToList();

                orderListVM.Orders.Add(individual);
            }

            var count = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders
                .OrderBy(p => p.OrderHeader.Id)
                .Skip((productPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            orderListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count
            };

            return View(orderListVM);
        }

        [Authorize(Roles = SD.ManagerUser)]
        public IActionResult ManageOrder()
        {
            List<OrderDetailsViewModel> orderDetailsVMList = new List<OrderDetailsViewModel>();

            List<OrderHeader> orderHeadersList = _db.OrderHeader
                    .Where(p => p.Status == SD.StatusSubmitted || p.Status == SD.StatusInProcess)
                    .OrderBy(p => p.PickupTime)
                    .ToList();

            foreach (OrderHeader item in orderHeadersList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel();
                individual.OrderHeader = item;
                individual.OrderDetails = _db.OrderDetails
                        .Where(p => p.OrderId == item.Id)
                        .ToList();

                orderDetailsVMList.Add(individual);
            }

            return View(orderDetailsVMList);
        }

        [Authorize(Roles = SD.ManagerUser)]
        public async Task<IActionResult> OrderPrepare(string orderId)
        {
            OrderHeader orderHeader = _db.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusInProcess;
            await _db.SaveChangesAsync();

            return RedirectToAction("ManageOrder", "Orders");
        }

        [Authorize(Roles = SD.ManagerUser)]
        public async Task<IActionResult> OrderCancel(string orderId)
        {
            OrderHeader orderHeader = _db.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusCancelled;
            await _db.SaveChangesAsync();

            return RedirectToAction("ManageOrder", "Orders");
        }

        [Authorize(Roles = SD.ManagerUser)]
        public async Task<IActionResult> OrderReady(string orderId)
        {
            OrderHeader orderHeader = _db.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusReady;
            await _db.SaveChangesAsync();

            return RedirectToAction("ManageOrder", "Orders");
        }

        // GET Order Pickup
        public IActionResult OrderPickup(
            string searchEmail = null,
            string searchPhone = null,
            string searchOrder = null)
        {
            List<OrderDetailsViewModel> orderDetailsVMList = new List<OrderDetailsViewModel>();
            if (searchEmail != null || searchPhone != null || searchOrder != null)
            {
                // Filtering the criteria
                var userByEmail = new ApplicationUser();
                var userByPhone = new ApplicationUser();
                List<OrderHeader> orderHeadersList = new List<OrderHeader>();

                if (searchOrder != null)
                {
                    orderHeadersList = _db.OrderHeader.Where(p => p.Id.Contains(searchOrder)).ToList();
                }
                if (searchEmail != null)
                {
                    userByEmail = _db.Users.Where(p => p.Email.ToLower().Contains(searchEmail.ToLower())).FirstOrDefault();
                }
                if (searchPhone != null)
                {
                    userByPhone = _db.Users.Where(p => p.PhoneNumber.ToLower().Contains(searchPhone.ToLower())).FirstOrDefault();
                }

                string userIdByEmail = "";
                string userIdByPhone = "";
                
                if (orderHeadersList.Count == 0)
                {
                    if (userByEmail != null)
                    {
                        userIdByEmail = userByEmail.Id;
                    }
                    if (userByPhone != null)
                    {
                        userIdByPhone = userByPhone.Id;
                    }

                    orderHeadersList = _db.OrderHeader
                            .Where(p => p.UserId == userIdByEmail || p.UserId == userIdByPhone)
                            .OrderByDescending(p => p.OrderDate)
                            .ToList();
                }

                foreach (OrderHeader itemOH in orderHeadersList)
                {
                    OrderDetailsViewModel individual = new OrderDetailsViewModel
                    {
                        OrderHeader = itemOH,
                        OrderDetails = _db.OrderDetails
                            .Where(p => p.OrderId == itemOH.Id)
                            .ToList()
                    };

                    orderDetailsVMList.Add(individual);
                }

            }
            else
            {
                // No filtering the criteria
                List<OrderHeader> orderHeadersList = _db.OrderHeader
                        .Where(p => p.Status == SD.StatusReady)
                        .OrderBy(p => p.PickupTime)
                        .ToList();

                foreach (OrderHeader item in orderHeadersList)
                {
                    OrderDetailsViewModel individual = new OrderDetailsViewModel
                    {
                        OrderHeader = item,
                        OrderDetails = _db.OrderDetails
                            .Where(p => p.OrderId == item.Id)
                            .ToList()
                    };

                    orderDetailsVMList.Add(individual);
                }
            }

            return View(orderDetailsVMList);
        }

        [Authorize(Roles = SD.ManagerUser)]
        public IActionResult OrderPickupDetails(string orderId)
        {
            OrderDetailsViewModel orderDetailsVM = new OrderDetailsViewModel
            {
                OrderHeader = _db.OrderHeader.Where(p => p.Id == orderId).FirstOrDefault()
            };

            orderDetailsVM.OrderHeader.ApplicationUser = _db.Users
                    .Where(p => p.Id == orderDetailsVM.OrderHeader.UserId)
                    .FirstOrDefault();

            orderDetailsVM.OrderDetails = _db.OrderDetails
                    .Where(p => p.OrderId == orderDetailsVM.OrderHeader.Id)
                    .ToList();

            return View(orderDetailsVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.ManagerUser)]
        [ActionName("OrderPickupDetails")]
        public async Task<IActionResult> OrderPickupDetailsPost(string orderId)
        {
            OrderHeader orderHeader = _db.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusCompleted;
            await _db.SaveChangesAsync();

            return RedirectToAction("OrderPickup", "Orders");

        }

        public IActionResult OrderSummaryExport(string orderId)
        {
            return View();
        }

        [HttpPost]
        public IActionResult OrderSummaryExport(OrderExportViewModel orderExportVM)
        {
            List<OrderHeader> orderHeaderList = _db.OrderHeader
                    .Where(p => p.OrderDate >= orderExportVM.startDate && p.OrderDate <= orderExportVM.endDate)
                    .ToList();

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            List<OrderDetails> individualList = new List<OrderDetails>();

            foreach (var orderHeaderItem in orderHeaderList)
            {
                individualList = _db.OrderDetails.Where(p => p.OrderId == orderHeaderItem.Id).ToList();

                foreach(var individualItem in individualList)
                {
                    orderDetailsList.Add(individualItem);
                }
            }

            byte[] bytes = Encoding.ASCII.GetBytes(ConvertListToString(orderDetailsList));
            string fileExportName = "OrderDetails_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".csv";
            return File(bytes, "application/text", fileExportName);
        }

        public string ConvertListToString<T>(List<T> data)
        {
            PropertyDescriptorCollection modelProperties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach(PropertyDescriptor propItem in modelProperties)
            {
                table.Columns.Add(propItem.Name, Nullable.GetUnderlyingType(propItem.PropertyType) ?? propItem.PropertyType);
            }

            foreach(T item in data)
            {
                DataRow row = table.NewRow();
                foreach(PropertyDescriptor propItem in modelProperties)
                {
                    row[propItem.Name] = propItem.GetValue(item) ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            table.Columns.Remove("OrderHeader");
            table.Columns.Remove("MenuItemId");
            table.Columns.Remove("MenuItem");
            table.Columns.Remove("Description");

            StringBuilder sb = new StringBuilder();
            IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().Select(col => col.ColumnName);

            sb.AppendLine(string.Join(",", columnNames));
            foreach(DataRow rowItem in table.Rows)
            {
                IEnumerable<string> fields = rowItem.ItemArray.Select(f => f.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            return sb.ToString();
        }

    }
}