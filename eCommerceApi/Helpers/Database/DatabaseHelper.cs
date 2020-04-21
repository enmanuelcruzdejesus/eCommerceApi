
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

        public static ProductCategory GetCategoryByRef(int IdRef)
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
            var result = new Customer();
            result.customerRef = customer.id.ToString();
            result.user_name = customer.username;
            result.customer_name = customer.first_name + " " + customer.last_name;
            if (customer.billing != null)
            {
                result.country = customer.billing.country;
                result.city = customer.billing.city;
                result.state = customer.billing.state;
                result.postcode = customer.billing.postcode;
                result.phone = customer.billing.phone;
                
            }

            result.role = customer.role;
            result.email = customer.email;
            result.avatar_url = customer.avatar_url;
            result.created = DateTime.Now;
            result.lastupdate = DateTime.Now;

            

            return result;
        }

        public static eCommerceApi.Model.ProductCategory GetCategoryFromEProductCategory(WooCommerceNET.WooCommerce.v3.ProductCategory category)
        {
            var result = new ProductCategory();
            result.categoryRef = Convert.ToInt32(category.id);
            result.descrip = category.description;
            result.slug = category.slug;
            return result;

        }
        
        public static eCommerceApi.Model.Product GetProductFromEProduct(WooCommerceNET.WooCommerce.v3.Product product)
        {
            eCommerceApi.Model.Product result = new Product();
            result.productRef = Convert.ToInt32(product.id);
            result.description = product.description;
            result.shortdescrip = product.short_description;
            
            if(product.categories.Count > 0)
            {
                var category = GetCategoryByRef(Convert.ToInt32(product.categories[0].id));
                result.categoryId = category.id;
          
            }
            result.price = Convert.ToDecimal(product.price);
            result.regular_price = Convert.ToDecimal(product.regular_price);
            result.sale_price = Convert.ToDecimal(product.sale_price);
            result.price_html = product.price_html;
            result.on_sale = Convert.ToBoolean(product.on_sale);
            result.purchasable = Convert.ToBoolean(product.purchasable);
            result.total_sales = Convert.ToDecimal(product.total_sales);
            result.taxt_status = product.tax_status;
            result.manage_stock = Convert.ToBoolean(product.manage_stock);
            result.stock_quantity = Convert.ToInt32(product.stock_quantity);
            result.stock_status = product.stock_status;
            result.backorders_allowed = Convert.ToBoolean(product.backorders_allowed);
            result.weight = Convert.ToDecimal(product.weight);
            if(product.dimensions!= null)
            {
                result.width = Convert.ToDecimal(product.dimensions.width);
                result.length = Convert.ToDecimal(product.dimensions.length);
                result.height = Convert.ToDecimal(product.dimensions.height);
            }
            if(product.attributes != null)
            {
                result.position = Convert.ToInt32(product.attributes[0].position);
                result.visible = Convert.ToBoolean(product.attributes[0].visible);
            }

            result.shipping_required = Convert.ToBoolean(product.shipping_required);
            result.shipping_taxable = Convert.ToBoolean(product.shipping_taxable);
            result.shipping_class_id = Convert.ToInt32(product.shipping_class_id);
            result.average_rating = product.average_rating;
            result.rating_count = Convert.ToInt32(product.rating_count);
            result.menu_order = Convert.ToInt32(product.menu_order);
            result.status = product.status;
            result.created = DateTime.Now;
            result.lastupdate = DateTime.Now;

            return result;
        }

        public static eCommerceApi.Model.Order GetOrderFromEOrder(WooCommerceNET.WooCommerce.v3.Order order)
        {
            eCommerceApi.Model.Order result = new Model.Order();
            result.orderRef = Convert.ToInt32(order.id);
            result.parentId = Convert.ToInt32(order.parent_id);
            result.order_key = order.order_key;
            result.order_number = order.number;
            result.customerId = Convert.ToInt32(order.customer_id);
            result.customer_notes = order.customer_note;
            result.order_date = Convert.ToDateTime(order.date_created);
            result.date_created_gmt = Convert.ToDateTime(order.date_created_gmt);
            result.date_paid = Convert.ToDateTime(order.date_paid);
            result.date_completed = Convert.ToDateTime(order.date_completed);
            result.currency = order.currency;
            result.payment_menthod = order.payment_method;
            result.payment_menthod_title = order.payment_method_title;
            result.discount_total = Convert.ToDecimal(order.discount_total);
            result.discount_tax = Convert.ToDecimal(order.discount_tax);
            result.shipping_total = Convert.ToDecimal(order.shipping_total);
            result.prices_include_tax = Convert.ToBoolean(order.prices_include_tax);
            if (order.tax_lines != null)
            {
                result.rateId = Convert.ToInt32(order.tax_lines[0].rate_id);
                result.rate_code = order.tax_lines[0].rate_code;
                result.tax_rate_label = order.tax_lines[0].label;

            }

            result.shipping_tax = Convert.ToDecimal(order.shipping_tax);
            result.total_tax = Convert.ToDecimal(order.total_tax);
            result.discount_tax = Convert.ToDecimal(order.discount_tax);
            result.total = Convert.ToDecimal(order.total);
            result.total_tax = Convert.ToDecimal(order.total_tax);
            
            if(order.billing != null)
            {
                result.first_name = order.billing.first_name;
                result.last_name = order.billing.last_name;
                result.company = order.billing.company;
                result.address1 = order.billing.address_1;
                result.address2 = order.billing.address_2;
                result.country = order.billing.country;
                result.city = order.billing.city;
                result.state = order.billing.state;
                result.postcode = order.billing.postcode;
                result.email = order.billing.email;
                result.phone = order.billing.phone;
                
            }
            result.status = order.status;
            result.created = DateTime.Now;
            result.lastupdate = DateTime.Now;


            if(order.line_items!= null)
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
                result.Detail = details;

            }




          
            return result;



            
        }
    }
}
