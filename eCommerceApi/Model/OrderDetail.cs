using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerceApi.Model
{
    [Alias("OrderDetails")]
    public class OrderDetail
    {

        [PrimaryKey]
        [AutoIncrement]
        public int id { get; set; }

        public int orderId { get; set; }
        public int productId { get; set; }
        public string descrip { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public decimal subtotal { get; set; }
        public string tax_class { get; set; }
        public decimal subtotal_tax { get; set; }
        public decimal total_tax { get; set; }
        public decimal total { get; set; }

       
        [Reference]
        public virtual Order Order { get; set; }


        [Reference]
        public virtual Product Item { get; set; }
    }
}
