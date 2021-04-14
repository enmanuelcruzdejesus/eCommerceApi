using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace eCommerce.Model.Entities
{
    [Alias("Customers")]
    public class Customers
    {
        [PrimaryKey]
        [AutoIncrement]
        public int id { get; set; }
        public string customerRef { get; set; }

        public string sku { get; set; }
        public string user_name { get; set; }

        public string password { get; set; }

        public string customer_name { get; set; }
        public string company { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
        public string phone { get; set; }
        public string role { get; set; }
        public string email { get; set; }
        public string avatar_url { get; set; }

        public bool is_paying_customer { get; set; }

        public DateTime created { get; set; }
        public DateTime lastupdate { get; set; }

        //public virtual ICollection<Orders> Orders { get; set; }
    }
}
