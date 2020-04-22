
using ApiCore;
using eCommerceApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace eCommerceApi.Helpers.Database
{
    public class DatabaseHelper
    {
        public static Product GetProductByRef(int IdRef)
        {
            var query = AppConfig.Instance().Db.Products.Get(p => p.productRef == IdRef).FirstOrDefault();
            return query;

        }

        public static ProductCategories GetCategoryByRef(int IdRef)
        {
            var query = AppConfig.Instance().Db.ProductCategories.Get(c => c.categoryRef == IdRef).FirstOrDefault();
            return query;

        }

        public static Customer GetCustomerByRef(int IdRef)
        {
            var query = AppConfig.Instance().Db.Customers.Get(c => c.customerRef == IdRef.ToString()).FirstOrDefault();
            return query;

        }


        public static eCommerceApi.Model.Customer GetCustomerFromECustomer(WooCommerceNET.WooCommerce.v3.Customer customer)
        {
            var obj = new Customer();
            obj.customer_name = customer.first_name + " " + customer.last_name;
            if (customer.billing != null)
            {
                obj.country = customer.billing.country;
                obj.city = customer.billing.city;
                obj.state = customer.billing.state;
                obj.postcode = customer.billing.postcode;
                obj.phone = customer.billing.phone;


            }

            obj.role = customer.role;
            obj.email = customer.email;
            obj.avatar_url = customer.avatar_url;


            var any = GetCustomerByRef(Convert.ToInt32(customer.id));
            if ( any != null)
            {

                obj.id = any.id;
                obj.customerRef = any.customerRef;
                obj.lastupdate = DateTime.Now;
              
            }
            else
            {
                obj.customerRef = customer.id.ToString();
                obj.created = DateTime.Now;
                obj.lastupdate = DateTime.Now;
            }

            return obj;
  
        }

        public static eCommerceApi.Model.ProductCategories GetCategoryFromEProductCategory(WooCommerceNET.WooCommerce.v3.ProductCategory category)
        {
            var obj = new ProductCategories();
            obj.descrip = category.description;
            obj.slug = category.slug;


            var any = GetCategoryByRef(Convert.ToInt32(category.id));
            if (any != null)
            {
                obj.id = any.id;
                obj.categoryRef = any.categoryRef;
                obj.lastupdate = DateTime.Now;
         
             
            }
            else
            {
                obj.categoryRef = Convert.ToInt32(category.id);
                obj.created = DateTime.Now;
                obj.lastupdate = DateTime.Now;
            }



            return obj;

        }

        public static eCommerceApi.Model.Product GetProductFromEProduct(WooCommerceNET.WooCommerce.v3.Product product)
        {
            var obj = new Product();
            obj.description = product.description;
            obj.shortdescrip = product.short_description;

            if (product.categories.Count > 0)
            {
                var category = GetCategoryByRef(Convert.ToInt32(product.categories[0].id));
                obj.categoryId = category.id;

            }
            obj.price = Convert.ToDecimal(product.price);
            obj.regular_price = Convert.ToDecimal(product.regular_price);
            obj.sale_price = Convert.ToDecimal(product.sale_price);
            obj.price_html = product.price_html;
            obj.on_sale = Convert.ToBoolean(product.on_sale);
            obj.purchasable = Convert.ToBoolean(product.purchasable);
            obj.total_sales = Convert.ToDecimal(product.total_sales);
            obj.taxt_status = product.tax_status;
            obj.manage_stock = Convert.ToBoolean(product.manage_stock);
            obj.stock_quantity = Convert.ToInt32(product.stock_quantity);
            obj.stock_status = product.stock_status;
            obj.backorders_allowed = Convert.ToBoolean(product.backorders_allowed);
            obj.weight = Convert.ToDecimal(product.weight);
            if (product.dimensions != null)
            {
                obj.width = Convert.ToDecimal(product.dimensions.width);
                obj.length = Convert.ToDecimal(product.dimensions.length);
                obj.height = Convert.ToDecimal(product.dimensions.height);
            }
            if (product.attributes != null)
            {
                obj.position = Convert.ToInt32(product.attributes[0].position);
                obj.visible = Convert.ToBoolean(product.attributes[0].visible);
            }

            obj.shipping_required = Convert.ToBoolean(product.shipping_required);
            obj.shipping_taxable = Convert.ToBoolean(product.shipping_taxable);
            obj.shipping_class_id = Convert.ToInt32(product.shipping_class_id);
            obj.average_rating = product.average_rating;
            obj.rating_count = Convert.ToInt32(product.rating_count);
            obj.menu_order = Convert.ToInt32(product.menu_order);
            obj.status = product.status;

            var any = GetProductByRef(Convert.ToInt32(product.id));
            if (any != null)
            {
                obj.id = any.id;
                obj.productRef = any.productRef;
                obj.lastupdate = DateTime.Now;


            }
            else
            {
                obj.productRef = Convert.ToInt32(product.id);
                obj.created = DateTime.Now;
                obj.lastupdate = DateTime.Now;
            }

            return obj;

        }

        public static eCommerceApi.Model.Order GetOrderFromEOrder(WooCommerceNET.WooCommerce.v3.Order order)
        {

            var obj = new Order();

            obj.orderRef = Convert.ToInt32(order.id);
            obj.parentId = Convert.ToInt32(order.parent_id);
            obj.order_key = order.order_key;
            obj.order_number = order.number;
            obj.customerId = Convert.ToInt32(order.customer_id);
            obj.customer_notes = order.customer_note;
            obj.order_date = Convert.ToDateTime(order.date_created);
            obj.date_created_gmt = Convert.ToDateTime(order.date_created_gmt);
            obj.date_paid = Convert.ToDateTime(order.date_paid);
            obj.date_completed = Convert.ToDateTime(order.date_completed);
            obj.currency = order.currency;
            obj.payment_menthod = order.payment_method;
            obj.payment_menthod_title = order.payment_method_title;
            obj.discount_total = Convert.ToDecimal(order.discount_total);
            obj.discount_tax = Convert.ToDecimal(order.discount_tax);
            obj.shipping_total = Convert.ToDecimal(order.shipping_total);
            obj.prices_include_tax = Convert.ToBoolean(order.prices_include_tax);
            if (order.tax_lines != null)
            {
                obj.rateId = Convert.ToInt32(order.tax_lines[0].rate_id);
                obj.rate_code = order.tax_lines[0].rate_code;
                obj.tax_rate_label = order.tax_lines[0].label;

            }

            obj.shipping_tax = Convert.ToDecimal(order.shipping_tax);
            obj.total_tax = Convert.ToDecimal(order.total_tax);
            obj.discount_tax = Convert.ToDecimal(order.discount_tax);
            obj.total = Convert.ToDecimal(order.total);
            obj.total_tax = Convert.ToDecimal(order.total_tax);

            if (order.billing != null)
            {
                obj.first_name = order.billing.first_name;
                obj.last_name = order.billing.last_name;
                obj.company = order.billing.company;
                obj.address1 = order.billing.address_1;
                obj.address2 = order.billing.address_2;
                obj.country = order.billing.country;
                obj.city = order.billing.city;
                obj.state = order.billing.state;
                obj.postcode = order.billing.postcode;
                obj.email = order.billing.email;
                obj.phone = order.billing.phone;

            }
            obj.status = order.status;


            if (order.line_items != null)
            {
                List<OrderDetail> details = new List<OrderDetail>();
                foreach (var item in order.line_items)
                {
                    OrderDetail i = new OrderDetail();

                    i.id = 0;
                    var product = GetProductByRef(Convert.ToInt32(item.id));
                    if (product != null)
                    {
                        i.productId = product.id;
                        i.descrip = product.description;

                    }

                    i.quantity = Convert.ToInt32(item.quantity);
                    i.price = Convert.ToDecimal(item.price);
                    i.subtotal = Convert.ToDecimal(item.subtotal);
                    i.tax_class = item.tax_class;
                    i.subtotal_tax = Convert.ToDecimal(item.subtotal_tax);
                    i.total_tax = Convert.ToDecimal(item.total_tax);
                    i.total = Convert.ToDecimal(item.total);

                    details.Add(i);

                }
                obj.Detail = details;

            }






            return obj;



            
        }
    }
}
