using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TastyFood.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TastyFood.Controllers.APIs
{
    [Route("api/[controller]")]
    public class OrderHeaderAPIController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OrderHeaderAPIController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get(string type, string query = null)
        {
            if (type.Equals("searchOrderId") && query != null)
            {
                var customerQuery = _db.OrderHeader
                        .Where(p => p.Id.ToLower().Contains(query.ToLower()));

                return Ok(customerQuery.ToList());
            }            

            return Ok();
        }

    }
}
