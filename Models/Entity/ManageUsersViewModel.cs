using CRMapi.Models.Entity;
using System.Collections.Generic;

namespace AspNetCoreTodo.Models
{
    public class ManageUsersViewModel
    {
        public Personal[] Administrators { get; set; }

        public Personal[] Everyone { get; set; }
    }
}
