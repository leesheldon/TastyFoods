using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TastyFood.Data;
using TastyFood.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TastyFood.Controllers.APIs
{
    [Route("api/[controller]")]
    public class CouponAPIController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponAPIController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get(double orderTotal, string couponCode = null)
        {
            // Return string will have :E for error and :S for success at the end.
            var rtn = "";
            if (couponCode == null)
            {
                rtn = orderTotal + ":E";
                return Ok(rtn);
            }

            var couponFromDB = _db.Coupon
                    .Where(p => p.Name == couponCode)
                    .FirstOrDefault();

            if (couponFromDB == null)
            {
                rtn = orderTotal + ":E";
                return Ok(rtn);
            }

            if (couponFromDB.MinimumAmount > orderTotal)
            {
                rtn = orderTotal + ":E";
                return Ok(rtn);
            }

            if (Convert.ToInt32(couponFromDB.CouponType)==(int)Coupon.ECouponType.Dollar)
            {
                orderTotal = orderTotal - couponFromDB.Discount;

                rtn = orderTotal + ":S";
                return Ok(rtn);
            }
            else
            {
                if (Convert.ToInt32(couponFromDB.CouponType) == (int)Coupon.ECouponType.Percent)
                {
                    orderTotal = orderTotal - (orderTotal * couponFromDB.Discount / 100);

                    rtn = orderTotal + ":S";
                    return Ok(rtn);
                }
            }

            return Ok();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
