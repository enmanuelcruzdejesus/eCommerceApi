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
    public class OrderDetails
    {

        [PrimaryKey]
        [AutoIncrement]
        public int id { get; set; }

        [Alias("orderId")]
        public int OrdersId { get; set; }

        public int orderId { get; set; }
        public int productId { get; set; }
        public string descrip { get; set; }
        public decimal quantity { get; set; }
        public decimal price { get; set; }
        public decimal subtotal { get; set; }
        public string tax_class { get; set; }
        public decimal subtotal_tax { get; set; }
        public decimal total_tax { get; set; }
        public decimal total { get; set; }
        public DateTime created { get; set; }
        public DateTime lastupdate { get; set; }


        //[Reference]
        //public virtual Orders Order { get; set; }


        //[Reference]
        //public virtual Products Item { get; set; }
    }
}
