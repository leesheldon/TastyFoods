﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TastyFood.Models.OrderDetailsViewModels
{
    public class OrderDetailsViewModel
    {
        public OrderHeader OrderHeader { get; set; }

        public List<OrderDetails> OrderDetails { get; set; }

    }
}