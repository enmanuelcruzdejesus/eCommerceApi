using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerceApi.Model
{
    public class OrderDetail
    {
        public int id { get; set; }
        public int productId { get; set; }
        public string descrip { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public int subtotal { get; set; }
        public string tax_class { get; set; }
        public decimal subtotal_tax { get; set; }
        public decimal total_tax { get; set; }
        public decimal total { get; set; }
    }
}
