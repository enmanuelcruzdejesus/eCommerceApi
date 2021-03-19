using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerceApi.Model
{
  
    public class Products
    {
        [PrimaryKey]
        [AutoIncrement]
        public int id { get; set; }
        public int productRef { get; set; }
        public string description { get; set; }
        public string shortdescrip { get; set; }

        public string sku { get; set; }
        public int categoryId { get; set; }
        public decimal price { get; set; }
        public decimal regular_price { get; set; }
        public decimal sale_price { get; set; }
        public string price_html { get; set; }
        public bool on_sale { get; set; }
        public bool purchasable { get; set; }
        public decimal total_sales { get; set; }
        public string taxt_status { get; set; }
        public bool manage_stock { get; set; }
        public int stock_quantity { get; set; }
        public string stock_status { get; set; }
        public bool backorders_allowed { get; set; }
        public decimal weight { get; set; }
        public decimal length { get; set; }
        public decimal width { get; set; }
        public decimal height { get; set; }
        public int position { get; set; }
        public bool visible { get; set; }
        public bool shipping_required { get; set; }
        public bool shipping_taxable { get; set; }
        public int shipping_class_id { get; set; }
        public bool reviews_allowed { get; set; }
        public string average_rating { get; set; }
        public int rating_count { get; set; }
        public int menu_order { get; set; }
        public string status { get; set; }
        public DateTime created { get; set; }
        public DateTime lastupdate { get; set; }

        //[Reference]
        //public virtual ICollection<OrderDetail> Orders { get; set; }
    }
}
