using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TastyFood.Models.UsersViewModels
{
    public class UserViewModel
    {
        public ApplicationUser SelectedUser { get; set; }

        public List<RolesListOfSelectedUser> RolesList { get; set; }

    }
}
