using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TastyFood.Utility
{
    public class SD
    {
        public const string DefaultFoodImage = "default_food.png";

        public const string SessionCountCarts = "CountCarts";

        // Roles Name
        public const string AdminEndUser = "Admin";

        public const string CustomerEndUser = "Customer";

        public const string ManagerUser = "Manager";

        public const string StaffUser = "Staff";

        public const string DelivererUser = "Deliverer";

        public const string NAUser = "NA";

        // Orders Status 
        public const string StatusSubmitted= "Submitted";
        public const string StatusInProcess = "Being Prepared";
        public const string StatusReady = "Ready for Pickup";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";

    }
}
