using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TastyFood.Models
{
    [NotMapped]
    public class RolesListOfSelectedUser
    {
        public string Id { get; set; }

        public string Name { get; set; }
        
        public bool SelectedRole { get; set; }

    }
}
