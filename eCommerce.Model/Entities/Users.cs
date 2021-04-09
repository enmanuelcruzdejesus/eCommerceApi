using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace eCommerce.Model.Entities
{
    [Alias("ApiUsers")]
    public class Users
    {
        public int id { get; set; }
        public String username { get; set; }
        public String password { get; set; }
        public bool active { get; set; }
    }
}
