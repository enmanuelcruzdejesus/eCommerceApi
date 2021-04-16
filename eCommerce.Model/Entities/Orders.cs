using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace eCommerce.Model.Entities
{
    public class Orders
    {

        [PrimaryKey]
        [AutoIncrement]
        [DataMember]
        public int id { get; set; }
        public int orderRef { get; set; }
        public int parentId { get; set; }
        public string order_key { get; set; }
        public string order_number { get; set; }

        public int sku { get; set; }
        public int customerId { get; set; }
        public string customer_notes { get; set; }
        public DateTime order_date { get; set; }
        public DateTime date_created_gmt { get; set; }
        public DateTime? date_paid { get; set; }
        public DateTime? date_completed { get; set; }
        public string currency { get; set; }
        public string payment_menthod { get; set; }
        public string payment_menthod_title { get; set; }
        public decimal discount_total { get; set; }
        public string  shipping_method { get; set; }
        public decimal shipping_total { get; set; }
        public bool prices_include_tax { get; set; }
        public int rateId { get; set; }
        public string rate_code { get; set; }
        public string tax_rate_label { get; set; }
        public decimal shipping_tax { get; set; }
        public decimal total_tax { get; set; }
        public decimal discount_tax { get; set; }
        public decimal total { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postcode { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string status { get; set; }
        public DateTime created { get; set; }
        public DateTime lastupdate { get; set; }

        [Reference]
        public virtual List<OrderDetails> Detail { get; set; }



        //[Reference]
        //public virtual Customers Customer { get; set; }
    }
}
