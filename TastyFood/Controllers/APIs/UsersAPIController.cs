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
    public class UsersAPIController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UsersAPIController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get(string type, string query = null)
        {
            if (type.Equals("email") && query != null)
            {
                var customerQuery = _db.Users
                        .Where(p => p.Email.ToLower().Contains(query.ToLower()));

                return Ok(customerQuery.ToList());
            }

            if (type.Equals("phone") && query != null)
            {
                var customerQuery = _db.Users
                        .Where(p => p.PhoneNumber.ToLower().Contains(query.ToLower()));

                return Ok(customerQuery.ToList());
            }

            return Ok();
        }

        
    }
}
