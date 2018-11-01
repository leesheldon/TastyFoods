using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TastyFood.Models.OrderDetailsViewModels;

namespace TastyFood.Models.OrderListViewModels
{
    public class OrderListViewModel
    {
        public IList<OrderDetailsViewModel> Orders { get; set; }

        public PagingInfo PagingInfo { get; set; }

    }
}
