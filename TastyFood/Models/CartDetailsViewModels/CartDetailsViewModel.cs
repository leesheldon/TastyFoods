using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TastyFood.Models.CartDetailsViewModels
{
    public class CartDetailsViewModel
    {
        public List<ShoppingCart> listCart { get; set; }

        public OrderHeader OrderHeader { get; set; }

    }
}
