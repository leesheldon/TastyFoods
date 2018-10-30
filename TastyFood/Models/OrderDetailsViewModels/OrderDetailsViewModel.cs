using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TastyFood.Models.OrderDetailsViewModels
{
    public class OrderDetailsViewModel
    {
        public List<ShoppingCart> listCart { get; set; }

        public OrderHeader OrderHeader { get; set; }

    }
}
