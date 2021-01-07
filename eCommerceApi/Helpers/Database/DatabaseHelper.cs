
using ApiCore;
using eCommerceApi.Model;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.Legacy;

namespace eCommerceApi.Helpers.Database
{
    public class DatabaseHelper
    {
        
       
        public static Orders GetOrderByRef(int IdRef)
        {
            var query = AppConfig.Instance().Db.Orders.Get(o => o.orderRef == IdRef).FirstOrDefault();
            return query;

        }

        public static Products GetProductByRef(int IdRef)
        {
            var query = AppConfig.Instance().Db.Products.Get(p => p.productRef == IdRef).FirstOrDefault();
            return query;

        }

        public static ProductCategories GetCategoryByRef(int IdRef)
        {
            var query = AppConfig.Instance().Db.ProductCategories.Get(c => c.categoryRef == IdRef).FirstOrDefault();
            return query;

        }

        public static Customers GetCustomerByRef(int IdRef)
        {
            var query = AppConfig.Instance().Db.Customers.Get(c => c.customerRef == IdRef.ToString()).FirstOrDefault();
            return query;

        }


        public static eCommerceApi.Model.Customers GetCustomerFromECustomer(WooCommerceNET.WooCommerce.v3.Customer customer)
        {
            var obj = new Customers();
            obj.customer_name = customer.first_name + " " + customer.last_name;
            obj.password = customer.password;
            if (customer.billing != null)
            {
                obj.company = customer.billing.company;
                obj.address1 = customer.billing.address_1;
                obj.address2 = customer.billing.address_2;
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
                obj.created = any.created;
                obj.lastupdate = DateTime.Now;
              
            }
            else
            {
                obj.user_name = customer.username;
                obj.password = customer.password;
                obj.customerRef = customer.id.ToString();
                obj.created = DateTime.Now;
                obj.lastupdate = DateTime.Now;
            }

            return obj;
  
        }

        public static eCommerceApi.Model.ProductCategories GetCategoryFromEProductCategory(WooCommerceNET.WooCommerce.v3.ProductCategory category)
        {
            var obj = new ProductCategories();
            obj.name = category.name;
            obj.descrip = category.description;
            obj.slug = category.slug;


            var any = GetCategoryByRef(Convert.ToInt32(category.id));
            if (any != null)
            {
                obj.id = any.id;
                obj.categoryRef = any.categoryRef;
                obj.created = any.created;
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

        public static eCommerceApi.Model.Products GetProductFromEProduct(WooCommerceNET.WooCommerce.v3.Product product)
        {
            var obj = new Products();
            obj.description = product.name;
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
                if(string.IsNullOrEmpty(product.dimensions.width) || string.IsNullOrWhiteSpace(product.dimensions.width))
                {
                    obj.width = 0;
                }
                else
                {
                    obj.width = Convert.ToDecimal(product.dimensions.width);
                }

                if (string.IsNullOrEmpty(product.dimensions.length) || string.IsNullOrWhiteSpace(product.dimensions.length))
                {
                    obj.width = 0;
                }
                else
                {
                    obj.length = Convert.ToDecimal(product.dimensions.length);
                }


                if (string.IsNullOrEmpty(product.dimensions.height) || string.IsNullOrWhiteSpace(product.dimensions.height))
                {
                    obj.height = 0;
                }
                else
                {
                    obj.height = Convert.ToDecimal(product.dimensions.height);
                }

             
              
               
            }
            if (product.attributes!= null)
            {
                if(product.attributes.Count > 0)
                {
                    obj.position = Convert.ToInt32(product.attributes[0].position);
                    obj.visible = Convert.ToBoolean(product.attributes[0].visible);
                }
              
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
                obj.created = any.created;
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

        public static eCommerceApi.Model.Orders GetOrderFromEOrder(WooCommerceNET.WooCommerce.v3.Order order)
        {
            var custId = GetCustomerByRef(Convert.ToInt32(order.customer_id)).id;

            var obj = new Orders();

            obj.orderRef = Convert.ToInt32(order.id);
            obj.parentId = Convert.ToInt32(order.parent_id);
            obj.order_key = order.order_key;
            obj.order_number = order.number;
            obj.customerId = Convert.ToInt32(custId);
            obj.customer_notes = order.customer_note;
            obj.order_date = Convert.ToDateTime(order.date_created);
            obj.date_created_gmt = Convert.ToDateTime(order.date_created_gmt);

            if (obj.date_paid != null)
                obj.date_paid = Convert.ToDateTime(order.date_paid);
            else
                obj.date_paid = null;

            if (obj.date_completed != null)
                obj.date_completed = Convert.ToDateTime(order.date_completed);
            else
                obj.date_completed = null;

         
         
            obj.currency = order.currency;
            obj.payment_menthod = order.payment_method;
            obj.payment_menthod_title = order.payment_method_title;
            obj.discount_total = Convert.ToDecimal(order.discount_total);
            obj.discount_tax = Convert.ToDecimal(order.discount_tax);
            obj.shipping_total = Convert.ToDecimal(order.shipping_total);
            obj.prices_include_tax = Convert.ToBoolean(order.prices_include_tax);
            if (order.tax_lines != null)
            {
                if(order.tax_lines.Count > 0)
                {
                    obj.rateId = Convert.ToInt32(order.tax_lines[0].rate_id);
                    obj.rate_code = order.tax_lines[0].rate_code;
                    obj.tax_rate_label = order.tax_lines[0].label;
                }
              

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


            if (order.line_items != null && order.line_items.Count > 0)
            {
                List<OrderDetails> details = new List<OrderDetails>();
                foreach (var item in order.line_items)
                {
                    OrderDetails i = new OrderDetails();

                    i.id = 0;
                    var product = GetProductByRef(Convert.ToInt32(item.product_id));
                    if (product != null)
                    {
                        i.productId = product.id;
                        i.descrip = product.description;

                    }

                    i.quantity = Convert.ToDecimal(item.quantity);
                    i.price = Convert.ToDecimal(item.price);
                    i.subtotal = Convert.ToDecimal(item.subtotal);
                    i.tax_class = item.tax_class;
                    i.subtotal_tax = Convert.ToDecimal(item.subtotal_tax);
                    i.total_tax = Convert.ToDecimal(item.total_tax);
                    i.total = Convert.ToDecimal(item.total);
                    i.created = DateTime.Now;
                    i.lastupdate = DateTime.Now;

                    details.Add(i);

                }
               
                obj.Detail = details;

            }




            obj.created = DateTime.Now;
            obj.lastupdate = DateTime.Now;

            return obj;



            
        }


        //

        public static WooCommerceNET.WooCommerce.v3.ProductCategory GetECategory(eCommerceApi.Model.ProductCategories category)
        {

            if (category != null)
            {
               
                var cat = new WooCommerceNET.WooCommerce.v3.ProductCategory();
                cat.id = category.categoryRef;
                cat.name = category.name;
                cat.slug = category.slug;
                cat.description = category.descrip;
                return cat;

            }
            return null;


        }
        public static WooCommerceNET.WooCommerce.v3.Product GetEProduct(eCommerceApi.Model.Products product)
        {

            if (product != null)
            {

                var db = AppConfig.Instance().Db;
                var category = db.ProductCategories.GetById(product.categoryId);
             
                var p = new WooCommerceNET.WooCommerce.v3.Product();
                p.id = product.productRef;
                p.name = product.description;                
                p.description = product.description;
                p.short_description = product.shortdescrip;
                p.categories = new List<WooCommerceNET.WooCommerce.v3.ProductCategoryLine>() 
                {
                    new WooCommerceNET.WooCommerce.v3.ProductCategoryLine()
                    {
                        id = category.categoryRef,
                        name = category.descrip,
                        slug = category.slug
                    }
                };
                p.price = product.price;
                p.regular_price = product.regular_price;
                p.sale_price = product.sale_price;
                p.on_sale = product.on_sale;
                p.purchasable = product.purchasable;
                p.total_sales = Convert.ToInt32(product.total_sales);
                p.tax_status = product.taxt_status;
                p.manage_stock = product.manage_stock;
                p.stock_quantity = product.stock_quantity;
                p.stock_status = product.stock_status;
                p.backorders_allowed = product.backorders_allowed;
                p.weight = product.weight;
                p.dimensions = new WooCommerceNET.WooCommerce.v3.ProductDimension()
                {
                    width = product.width.ToString(),
                    height = product.height.ToString(),
                    length = product.length.ToString()
                };

                p.attributes = new List<WooCommerceNET.WooCommerce.v3.ProductAttributeLine>()
                {
                  new WooCommerceNET.WooCommerce.v3.ProductAttributeLine()
                  {
                      position = product.position,
                      visible = product.visible
                  }
                };

                p.shipping_required = product.shipping_required;
                p.shipping_taxable = product.shipping_taxable;
                p.shipping_class_id = product.shipping_class_id.ToString();
                p.reviews_allowed = product.reviews_allowed;
                p.average_rating = product.average_rating;
                p.rating_count = product.rating_count;
                p.menu_order = product.menu_order;
                p.status = product.status;


                return p;
           

            }
            return null;


        }

        public static WooCommerceNET.WooCommerce.v3.Customer GetECustomer(eCommerceApi.Model.Customers customer)
        {
            if(customer!= null)
            {
                WooCommerceNET.WooCommerce.v3.Customer cust = new WooCommerceNET.WooCommerce.v3.Customer();
                cust.id = Convert.ToInt32(customer.customerRef);
                cust.username = customer.user_name;
                cust.password = customer.password;
                cust.first_name = customer.customer_name;
                cust.date_created_gmt = DateTime.Now;
                cust.is_paying_customer = true;
                cust.date_modified = DateTime.Now;
                cust.date_created = DateTime.Now;
               
                cust.billing = new WooCommerceNET.WooCommerce.v3.CustomerBilling()
                {
                    address_1 = customer.address1,
                    address_2 = customer.address2,
                    country = customer.country,
                    city = customer.city,
                    state = customer.state,
                    company = customer.company,
                    postcode = customer.postcode,
                    phone = customer.phone,
                    first_name  = customer.customer_name
                };
                cust.email = customer.email;
                cust.role = customer.role;

                return cust;

                
                
            }
            return null;


        }


        public static void CustomerBulkMerge(string connectionString, List<Customers> customers)
        {
            SqlConnection.ClearAllPools();
            using (var oConn = new SqlConnection(connectionString))
            {
                oConn.Open();

                var oCmd = new SqlCommand(@"MERGE customers WITH(HOLDLOCK) as dest
                                        USING (VALUES (
                                         @id,
                                         @customerRef,
                                         @user_name,
                                         @password,
                                         @customer_name,
                                         @company,
                                         @address1,
                                         @address2,
                                         @country,
                                         @city,
                                         @state,
                                         @postcode,
                                         @phone,
                                         @role,
                                         @email,
                                         @avatar_url,
                                         @created,
                                         @lastupdate))
                                            AS src 
                                            (
                                           id,
                                           customerRef,
                                           user_name,
                                           password,
                                           customer_name,
                                           company,
                                           address1,
                                           address2,
                                           country,
                                           city,
                                           state,
                                           postcode,
                                           phone,
                                           role,
                                           email,
                                           avatar_url,
                                           created,
                                           lastupdate
                                            )
                                            ON src.id = dest.id
                                        WHEN MATCHED THEN
                                            UPDATE SET 
	                                    [user_name]=src.user_name,
                                            [password]=src.password,
                                            [customer_name]=src.customer_name,
                                            [company]=src.company,
                                            [address1]=src.address1,
                                            [address2]=src.address2,
                                            [country]=src.country,
                                            [city]=src.city,
                                            [state]=src.state,
                                            [postcode]=src.postcode,
                                            [phone]=src.phone,
                                            [role]=src.role,
                                            [email]=src.email,
                                            [avatar_url]=src.avatar_url,
                                            [lastupdate]=src.lastupdate
                                            WHEN NOT MATCHED THEN
                                            INSERT VALUES (		                                                                                                                               
                                                       src.[customerRef],
                                                       src.[user_name],
                                                       src.[password],
                                                       src.[customer_name],
                                                       src.[company],
                                                       src.[address1],
                                                       src.[address2],
                                                       src.[country],
                                                       src.[city],
                                                       src.[state],
                                                       src.[postcode],
                                                       src.[phone],
                                                       src.[role],
                                                       src.[email],
                                                       src.[avatar_url],
                                                       src.[created],
                                                       src.[lastupdate]);",
                                                            oConn);

                var id = oCmd.CreateParameter(); id.ParameterName = "@id"; oCmd.Parameters.Add(id);
                var customerRef = oCmd.CreateParameter(); customerRef.ParameterName = "@customerRef"; oCmd.Parameters.Add(customerRef);
                var user_name = oCmd.CreateParameter(); user_name.ParameterName = "@user_name"; oCmd.Parameters.Add(user_name);
                var password = oCmd.CreateParameter(); password.ParameterName = "@password"; oCmd.Parameters.Add(password);
                var customer_name = oCmd.CreateParameter(); customer_name.ParameterName = "@customer_name"; oCmd.Parameters.Add(customer_name);
                var company = oCmd.CreateParameter(); company.ParameterName = "@company"; oCmd.Parameters.Add(company);
                var address1 = oCmd.CreateParameter(); address1.ParameterName = "@address1"; oCmd.Parameters.Add(address1);
                var address2 = oCmd.CreateParameter(); address2.ParameterName = "@address2"; oCmd.Parameters.Add(address2);
                var country = oCmd.CreateParameter(); country.ParameterName = "@country"; oCmd.Parameters.Add(country);
                var city = oCmd.CreateParameter(); city.ParameterName = "@city"; oCmd.Parameters.Add(city);
                var state = oCmd.CreateParameter(); state.ParameterName = "@state"; oCmd.Parameters.Add(state);
                var postcode = oCmd.CreateParameter(); postcode.ParameterName = "@postcode"; oCmd.Parameters.Add(postcode);
                var phone = oCmd.CreateParameter(); phone.ParameterName = "@phone"; oCmd.Parameters.Add(phone);
                var role = oCmd.CreateParameter(); role.ParameterName = "@role"; oCmd.Parameters.Add(role);
                var email = oCmd.CreateParameter(); email.ParameterName = "@email"; oCmd.Parameters.Add(email);
                var avatar_url = oCmd.CreateParameter(); avatar_url.ParameterName = "@avatar_url"; oCmd.Parameters.Add(avatar_url);
                var created = oCmd.CreateParameter(); created.ParameterName = "@created"; oCmd.Parameters.Add(created);
                var lastupdate = oCmd.CreateParameter(); lastupdate.ParameterName = "@lastupdate"; oCmd.Parameters.Add(lastupdate);

                foreach (var c in customers)
                {
                    id.Value = c.id;
                    customerRef.Value = c.customerRef;
                    user_name.Value = GetDataValue(c.user_name);
                    password.Value = GetDataValue(c.password);
                    customer_name.Value = c.customer_name;
                    company.Value = GetDataValue(c.company);
                    address1.Value = GetDataValue(c.address1);
                    address2.Value = GetDataValue(c.address2);
                    country.Value = GetDataValue(c.country);
                    city.Value = GetDataValue(c.city);
                    state.Value = GetDataValue(c.state);
                    postcode.Value = c.postcode;
                    phone.Value = GetDataValue(c.phone);
                    role.Value = GetDataValue(c.role);
                    email.Value = GetDataValue(c.email);
                    avatar_url.Value = GetDataValue(c.avatar_url);
                    created.Value = c.created;
                    lastupdate.Value = c.lastupdate;
                    oCmd.ExecuteNonQuery();
                }

                oCmd.Dispose();

                oConn.Close();
                oConn.Dispose();
            }
        }




        public static void ProductsBulkMerge(string connectionString, List<Products> products)
        {
            SqlConnection.ClearAllPools();
            using (var oConn = new SqlConnection(connectionString))
            {
                oConn.Open();

                var oCmd = new SqlCommand(@"MERGE products WITH(HOLDLOCK) as dest
                                        USING (VALUES (
                                          @id,
                                            @productRef,
                                            @description,
                                            @shortdescrip,
                                            @categoryId,
                                            @price,
                                            @regular_price,
                                            @sale_price,
                                            @price_html,
                                            @on_sale,
                                            @purchasable,
                                            @total_sales,
                                            @taxt_status,
                                            @manage_stock,
                                            @stock_quantity,
                                            @stock_status,
                                            @backorders_allowed,
                                            @weight,
                                            @length,
                                            @width,
                                            @height,
                                            @position,
                                            @visible,
                                            @shipping_required,
                                            @shipping_taxable,
                                            @shipping_class_id,
                                            @reviews_allowed,
                                            @average_rating,
                                            @rating_count,
                                            @menu_order,
                                            @status,
                                            @created,
                                            @lastupdate))
                                            AS src 
                                            (
                                            id,
                                            productRef,
                                            description,
                                            shortdescrip,
                                            categoryId,
                                            price,
                                            regular_price,
                                            sale_price,
                                            price_html,
                                            on_sale,
                                            purchasable,
                                            total_sales,
                                            taxt_status,
                                            manage_stock,
                                            stock_quantity,
                                            stock_status,
                                            backorders_allowed,
                                            weight,
                                            length,
                                            width,
                                            height,
                                            position,
                                            visible,
                                            shipping_required,
                                            shipping_taxable,
                                            shipping_class_id,
                                            reviews_allowed,
                                            average_rating,
                                            rating_count,
                                            menu_order,
                                            status,
                                            created,
                                            lastupdate
                                            )
                                            ON src.id = dest.id
                                        WHEN MATCHED THEN
                                            UPDATE SET 
	                                       [description]=src.description,
                                            [shortdescrip]=src.shortdescrip,
                                            [categoryId]=src.categoryId,
                                            [price]=src.price,
                                            [regular_price]=src.regular_price,
                                            [sale_price]=src.sale_price,
                                            [price_html]=src.price_html,
                                            [on_sale]=src.on_sale,
                                            [purchasable]=src.purchasable,
                                            [total_sales]=src.total_sales,
                                            [taxt_status]=src.taxt_status,
                                            [manage_stock]=src.manage_stock,
                                            [stock_quantity]=src.stock_quantity,
                                            [stock_status]=src.stock_status,
                                            [backorders_allowed]=src.backorders_allowed,
                                            [weight]=src.weight,
                                            [length]=src.length,
                                            [width]=src.width,
                                            [height]=src.height,
                                            [position]=src.position,
                                            [visible]=src.visible,
                                            [shipping_required]=src.shipping_required,
                                            [shipping_taxable]=src.shipping_taxable,
                                            [shipping_class_id]=src.shipping_class_id,
                                            [reviews_allowed]=src.reviews_allowed,
                                            [average_rating]=src.average_rating,
                                            [rating_count]=src.rating_count,
                                            [menu_order]=src.menu_order,
                                            [status]=src.status,
                                            [lastupdate]=src.lastupdate
                                            WHEN NOT MATCHED THEN
                                            INSERT VALUES 	(	                                                                                     
                                                        src.[productRef],
                                                        src.[description],
                                                        src.[shortdescrip],
                                                        src.[categoryId],
                                                        src.[price],
                                                        src.[regular_price],
                                                        src.[sale_price],
                                                        src.[price_html],
                                                        src.[on_sale],
                                                        src.[purchasable],
                                                        src.[total_sales],
                                                        src.[taxt_status],
                                                        src.[manage_stock],
                                                        src.[stock_quantity],
                                                        src.[stock_status],
                                                        src.[backorders_allowed],
                                                        src.[weight],
                                                        src.[length],
                                                        src.[width],
                                                        src.[height],
                                                        src.[position],
                                                        src.[visible],
                                                        src.[shipping_required],
                                                        src.[shipping_taxable],
                                                        src.[shipping_class_id],
                                                        src.[reviews_allowed],
                                                        src.[average_rating],
                                                        src.[rating_count],
                                                        src.[menu_order],
                                                        src.[status],
                                                        src.[created],
                                                        src.[lastupdate]);",
                                                            oConn);
                var id = oCmd.CreateParameter(); id.ParameterName = "@id"; oCmd.Parameters.Add(id);
                var productRef = oCmd.CreateParameter(); productRef.ParameterName = "@productRef"; oCmd.Parameters.Add(productRef);
                var description = oCmd.CreateParameter(); description.ParameterName = "@description"; oCmd.Parameters.Add(description);
                var shortdescrip = oCmd.CreateParameter(); shortdescrip.ParameterName = "@shortdescrip"; oCmd.Parameters.Add(shortdescrip);
                var categoryId = oCmd.CreateParameter(); categoryId.ParameterName = "@categoryId"; oCmd.Parameters.Add(categoryId);
                var price = oCmd.CreateParameter(); price.ParameterName = "@price"; oCmd.Parameters.Add(price);
                var regular_price = oCmd.CreateParameter(); regular_price.ParameterName = "@regular_price"; oCmd.Parameters.Add(regular_price);
                var sale_price = oCmd.CreateParameter(); sale_price.ParameterName = "@sale_price"; oCmd.Parameters.Add(sale_price);
                var price_html = oCmd.CreateParameter(); price_html.ParameterName = "@price_html"; oCmd.Parameters.Add(price_html);
                var on_sale = oCmd.CreateParameter(); on_sale.ParameterName = "@on_sale"; oCmd.Parameters.Add(on_sale);
                var purchasable = oCmd.CreateParameter(); purchasable.ParameterName = "@purchasable"; oCmd.Parameters.Add(purchasable);
                var total_sales = oCmd.CreateParameter(); total_sales.ParameterName = "@total_sales"; oCmd.Parameters.Add(total_sales);
                var taxt_status = oCmd.CreateParameter(); taxt_status.ParameterName = "@taxt_status"; oCmd.Parameters.Add(taxt_status);
                var manage_stock = oCmd.CreateParameter(); manage_stock.ParameterName = "@manage_stock"; oCmd.Parameters.Add(manage_stock);
                var stock_quantity = oCmd.CreateParameter(); stock_quantity.ParameterName = "@stock_quantity"; oCmd.Parameters.Add(stock_quantity);
                var stock_status = oCmd.CreateParameter(); stock_status.ParameterName = "@stock_status"; oCmd.Parameters.Add(stock_status);
                var backorders_allowed = oCmd.CreateParameter(); backorders_allowed.ParameterName = "@backorders_allowed"; oCmd.Parameters.Add(backorders_allowed);
                var weight = oCmd.CreateParameter(); weight.ParameterName = "@weight"; oCmd.Parameters.Add(weight);
                var length = oCmd.CreateParameter(); length.ParameterName = "@length"; oCmd.Parameters.Add(length);
                var width = oCmd.CreateParameter(); width.ParameterName = "@width"; oCmd.Parameters.Add(width);
                var height = oCmd.CreateParameter(); height.ParameterName = "@height"; oCmd.Parameters.Add(height);
                var position = oCmd.CreateParameter(); position.ParameterName = "@position"; oCmd.Parameters.Add(position);
                var visible = oCmd.CreateParameter(); visible.ParameterName = "@visible"; oCmd.Parameters.Add(visible);
                var shipping_required = oCmd.CreateParameter(); shipping_required.ParameterName = "@shipping_required"; oCmd.Parameters.Add(shipping_required);
                var shipping_taxable = oCmd.CreateParameter(); shipping_taxable.ParameterName = "@shipping_taxable"; oCmd.Parameters.Add(shipping_taxable);
                var shipping_class_id = oCmd.CreateParameter(); shipping_class_id.ParameterName = "@shipping_class_id"; oCmd.Parameters.Add(shipping_class_id);
                var reviews_allowed = oCmd.CreateParameter(); reviews_allowed.ParameterName = "@reviews_allowed"; oCmd.Parameters.Add(reviews_allowed);
                var average_rating = oCmd.CreateParameter(); average_rating.ParameterName = "@average_rating"; oCmd.Parameters.Add(average_rating);
                var rating_count = oCmd.CreateParameter(); rating_count.ParameterName = "@rating_count"; oCmd.Parameters.Add(rating_count);
                var menu_order = oCmd.CreateParameter(); menu_order.ParameterName = "@menu_order"; oCmd.Parameters.Add(menu_order);
                var status = oCmd.CreateParameter(); status.ParameterName = "@status"; oCmd.Parameters.Add(status);
                var created = oCmd.CreateParameter(); created.ParameterName = "@created"; oCmd.Parameters.Add(created);
                var lastupdate = oCmd.CreateParameter(); lastupdate.ParameterName = "@lastupdate"; oCmd.Parameters.Add(lastupdate);

                foreach (var item in products)
                {
                    id.Value = item.id;
                    productRef.Value = item.productRef;
                    description.Value = item.description;
                    shortdescrip.Value = item.shortdescrip;
                    categoryId.Value = item.categoryId;
                    price.Value = item.price;
                    regular_price.Value = item.regular_price;
                    sale_price.Value = item.sale_price;
                    price_html.Value = item.price_html;
                    on_sale.Value = item.on_sale;
                    purchasable.Value = item.purchasable;
                    total_sales.Value = GetDataValue(item.total_sales);
                    taxt_status.Value = GetDataValue(item.taxt_status);
                    manage_stock.Value = GetDataValue(item.manage_stock);
                    stock_quantity.Value = GetDataValue(item.stock_quantity);
                    stock_status.Value = GetDataValue(item.stock_status);
                    backorders_allowed.Value = item.backorders_allowed;
                    weight.Value = GetDataValue(item.weight);
                    length.Value = GetDataValue(item.length);
                    width.Value = GetDataValue(item.width);
                    height.Value = GetDataValue(item.height);
                    position.Value = GetDataValue(item.position);
                    visible.Value = item.visible;
                    shipping_required.Value = item.shipping_required;
                    shipping_taxable.Value = item.shipping_taxable;
                    shipping_class_id.Value = item.shipping_class_id;
                    reviews_allowed.Value = item.reviews_allowed;
                    average_rating.Value = item.average_rating;
                    rating_count.Value = item.rating_count;
                    menu_order.Value = item.menu_order;
                    status.Value = item.status;
                    created.Value = item.created;
                    lastupdate.Value = item.lastupdate;

                    oCmd.ExecuteNonQuery();
                }

                oCmd.Dispose();

                oConn.Close();
                oConn.Dispose();
            }
        }



        public static void ProductCategoriesBulkMerge(string connectionString, List<ProductCategories> productCategories)
        {
            SqlConnection.ClearAllPools();
            using (var oConn = new SqlConnection(connectionString))
            {
                oConn.Open();

                var oCmd = new SqlCommand(@"MERGE ProductCategories WITH(HOLDLOCK) as dest
                                        USING (VALUES (
                                             @id,
                                             @categoryRef,
                                             @name,
                                             @descrip,
                                             @slug,
                                             @created,
                                             @lastupdate))
                                            AS src 
                                            (
                                          id,
                                          categoryRef,
                                          name,
                                          descrip,
                                          slug,
                                          created,
                                          lastupdate
                                            )
                                            ON src.id = dest.id
                                        WHEN MATCHED THEN
                                            UPDATE SET 
                                            [name]=src.name,
	                                       [descrip]=src.descrip,
                                           [slug]=src.slug,                                     
                                           [lastupdate]=src.lastupdate                                     
                                            WHEN NOT MATCHED THEN
                                            INSERT VALUES 	
                                          (	    
                                
                                         src.[categoryRef],
                                         src.[name],
                                         src.[descrip],
                                         src.[slug],
                                         src.[created],
                                         src.[lastupdate]);",
                                                     oConn);

                var id = oCmd.CreateParameter(); id.ParameterName = "@id"; oCmd.Parameters.Add(id);
                var categoryRef = oCmd.CreateParameter(); categoryRef.ParameterName = "@categoryRef"; oCmd.Parameters.Add(categoryRef);
                var name = oCmd.CreateParameter(); name.ParameterName = "@name"; oCmd.Parameters.Add(name);
                var descrip = oCmd.CreateParameter(); descrip.ParameterName = "@descrip"; oCmd.Parameters.Add(descrip);
                var slug = oCmd.CreateParameter(); slug.ParameterName = "@slug"; oCmd.Parameters.Add(slug);
                var created = oCmd.CreateParameter(); created.ParameterName = "@created"; oCmd.Parameters.Add(created);
                var lastupdate = oCmd.CreateParameter(); lastupdate.ParameterName = "@lastupdate"; oCmd.Parameters.Add(lastupdate);

                foreach (var category in productCategories)
                {
                    id.Value = category.id;
                    categoryRef.Value = category.categoryRef;
                    name.Value = category.name;
                    descrip.Value = category.descrip;
                    slug.Value = GetDataValue(category.slug);
                    created.Value = category.created;
                    lastupdate.Value = category.lastupdate;
                    oCmd.ExecuteNonQuery();
                }

                oCmd.Dispose();

                oConn.Close();
                oConn.Dispose();
            }
        }


        public static void OrderBulkMerge(string connectionString, List<Orders> salesOrders)
        {
            SqlConnection.ClearAllPools();
            using (var oConn = new SqlConnection(connectionString))
            {
                oConn.Open();

                var oCmd = new SqlCommand(@"MERGE orders WITH(HOLDLOCK) as dest
                                        USING (VALUES (
                                              @id,
                                              @orderRef,
                                              @parentId,
                                              @order_key,
                                              @order_number,
                                              @customerId,
                                              @customer_notes,
                                              @order_date,
                                              @date_created_gmt,
                                              @date_paid,
                                              @date_completed,
                                              @currency,
                                              @payment_menthod,
                                              @payment_menthod_title,
                                              @discount_total,
                                              @shipping_total,
                                              @prices_include_tax,
                                              @rateId,
                                              @rate_code,
                                              @tax_rate_label,
                                              @shipping_tax,
                                              @total_tax,
                                              @discount_tax,
                                              @total,
                                              @first_name,
                                              @last_name,
                                              @company,
                                              @address1,
                                              @address2,
                                              @country,
                                              @city,
                                              @state,
                                              @postcode,
                                              @email,
                                              @phone,
                                              @status,
                                              @created,
                                              @lastupdate))
                                            AS src 
                                            (
                                             id,
                                             orderRef,
                                             parentId,
                                             order_key,
                                             order_number,
                                             customerId,
                                             customer_notes,
                                             order_date,
                                             date_created_gmt,
                                             date_paid,
                                             date_completed,
                                             currency,
                                             payment_menthod,
                                             payment_menthod_title,
                                             discount_total,
                                             shipping_total,
                                             prices_include_tax,
                                             rateId,
                                             rate_code,
                                             tax_rate_label,
                                             shipping_tax,
                                             total_tax,
                                             discount_tax,
                                             total,
                                             first_name,
                                             last_name,
                                             company,
                                             address1,
                                             address2,
                                             country,
                                             city,
                                             state,
                                             postcode,
                                             email,
                                             phone,
                                             status,
                                             created,
                                             lastupdate
                                            )
                                            ON src.id = dest.id
                                        WHEN MATCHED THEN
                                            UPDATE SET 
	                                         [order_key]=src.order_key
                                            ,[order_number]=src.order_number
                                            ,[customerId]=src.customerId
                                            ,[customer_notes]=src.customer_notes
                                            ,[order_date]=src.order_date
                                            ,[date_created_gmt]=src.date_created_gmt
                                            ,[date_paid]=src.date_paid
                                            ,[date_completed]=src.date_completed
                                            ,[currency]=src.currency
                                            ,[payment_menthod]=src.payment_menthod
                                            ,[payment_menthod_title]=src.payment_menthod_title
                                            ,[discount_total]=src.discount_total
                                            ,[shipping_total]=src.shipping_total
                                            ,[prices_include_tax]=src.prices_include_tax
                                            ,[rateId]=src.rateId
                                            ,[rate_code]=src.rate_code
                                            ,[tax_rate_label]=src.tax_rate_label
                                            ,[shipping_tax]=src.shipping_tax
                                            ,[total_tax]=src.total_tax
                                            ,[discount_tax]=src.discount_tax
                                            ,[total]=src.total
                                            ,[first_name]=src.first_name
                                            ,[last_name]=src.last_name
                                            ,[company]=src.company
                                            ,[address1]=src.address1
                                            ,[address2]=src.address2
                                            ,[country]=src.country
                                            ,[city]=src.city
                                            ,[state]=src.state
                                            ,[postcode]=src.postcode
                                            ,[email]=src.email
                                            ,[phone]=src.phone
                                            ,[status]=src.status                  
                                            ,[lastupdate]=src.lastupdate
                                            WHEN NOT MATCHED THEN
                                            INSERT VALUES (                                          
                                                   
                                                      src.[orderRef],
                                                      src.[parentId],
                                                      src.[order_key],
                                                      src.[order_number],
                                                      src.[customerId],
                                                      src.[customer_notes],
                                                      src.[order_date],
                                                      src.[date_created_gmt],
                                                      src.[date_paid],
                                                      src.[date_completed],
                                                      src.[currency],
                                                      src.[payment_menthod],
                                                      src.[payment_menthod_title],
                                                      src.[discount_total],
                                                      src.[shipping_total],
                                                      src.[prices_include_tax],
                                                      src.[rateId],
                                                      src.[rate_code],
                                                      src.[tax_rate_label],
                                                      src.[shipping_tax],
                                                      src.[total_tax],
                                                      src.[discount_tax],
                                                      src.[total],
                                                      src.[first_name],
                                                      src.[last_name],
                                                      src.[company],
                                                      src.[address1],
                                                      src.[address2],
                                                      src.[country],
                                                      src.[city],
                                                      src.[state],
                                                      src.[postcode],
                                                      src.[email],
                                                      src.[phone],
                                                      src.[status],
                                                      src.[created],
                                                      src.[lastupdate]);",
                                                     oConn);

                var id = oCmd.CreateParameter(); id.ParameterName = "@id"; oCmd.Parameters.Add(id);
                var orderRef = oCmd.CreateParameter(); orderRef.ParameterName = "@orderRef"; oCmd.Parameters.Add(orderRef);
                var parentId = oCmd.CreateParameter(); parentId.ParameterName = "@parentId"; oCmd.Parameters.Add(parentId);
                var order_key = oCmd.CreateParameter(); order_key.ParameterName = "@order_key"; oCmd.Parameters.Add(order_key);
                var order_number = oCmd.CreateParameter(); order_number.ParameterName = "@order_number"; oCmd.Parameters.Add(order_number);
                var customerId = oCmd.CreateParameter(); customerId.ParameterName = "@customerId"; oCmd.Parameters.Add(customerId);
                var customer_notes = oCmd.CreateParameter(); customer_notes.ParameterName = "@customer_notes"; oCmd.Parameters.Add(customer_notes);
                var order_date = oCmd.CreateParameter(); order_date.ParameterName = "@order_date"; oCmd.Parameters.Add(order_date);
                var date_created_gmt = oCmd.CreateParameter(); date_created_gmt.ParameterName = "@date_created_gmt"; oCmd.Parameters.Add(date_created_gmt);
                var date_paid = oCmd.CreateParameter(); date_paid.ParameterName = "@date_paid"; oCmd.Parameters.Add(date_paid);
                var date_completed = oCmd.CreateParameter(); date_completed.ParameterName = "@date_completed"; oCmd.Parameters.Add(date_completed);
                var currency = oCmd.CreateParameter(); currency.ParameterName = "@currency"; oCmd.Parameters.Add(currency);
                var payment_menthod = oCmd.CreateParameter(); payment_menthod.ParameterName = "@payment_menthod"; oCmd.Parameters.Add(payment_menthod);
                var payment_menthod_title = oCmd.CreateParameter(); payment_menthod_title.ParameterName = "@payment_menthod_title"; oCmd.Parameters.Add(payment_menthod_title);
                var discount_total = oCmd.CreateParameter(); discount_total.ParameterName = "@discount_total"; oCmd.Parameters.Add(discount_total);
                var shipping_total = oCmd.CreateParameter(); shipping_total.ParameterName = "@shipping_total"; oCmd.Parameters.Add(shipping_total);
                var prices_include_tax = oCmd.CreateParameter(); prices_include_tax.ParameterName = "@prices_include_tax"; oCmd.Parameters.Add(prices_include_tax);
                var rateId = oCmd.CreateParameter(); rateId.ParameterName = "@rateId"; oCmd.Parameters.Add(rateId);
                var rate_code = oCmd.CreateParameter(); rate_code.ParameterName = "@rate_code"; oCmd.Parameters.Add(rate_code);
                var tax_rate_label = oCmd.CreateParameter(); tax_rate_label.ParameterName = "@tax_rate_label"; oCmd.Parameters.Add(tax_rate_label);
                var shipping_tax = oCmd.CreateParameter(); shipping_tax.ParameterName = "@shipping_tax"; oCmd.Parameters.Add(shipping_tax);
                var total_tax = oCmd.CreateParameter(); total_tax.ParameterName = "@total_tax"; oCmd.Parameters.Add(total_tax);
                var discount_tax = oCmd.CreateParameter(); discount_tax.ParameterName = "@discount_tax"; oCmd.Parameters.Add(discount_tax);
                var total = oCmd.CreateParameter(); total.ParameterName = "@total"; oCmd.Parameters.Add(total);
                var first_name = oCmd.CreateParameter(); first_name.ParameterName = "@first_name"; oCmd.Parameters.Add(first_name);
                var last_name = oCmd.CreateParameter(); last_name.ParameterName = "@last_name"; oCmd.Parameters.Add(last_name);
                var company = oCmd.CreateParameter(); company.ParameterName = "@company"; oCmd.Parameters.Add(company);
                var address1 = oCmd.CreateParameter(); address1.ParameterName = "@address1"; oCmd.Parameters.Add(address1);
                var address2 = oCmd.CreateParameter(); address2.ParameterName = "@address2"; oCmd.Parameters.Add(address2);
                var country = oCmd.CreateParameter(); country.ParameterName = "@country"; oCmd.Parameters.Add(country);
                var city = oCmd.CreateParameter(); city.ParameterName = "@city"; oCmd.Parameters.Add(city);
                var state = oCmd.CreateParameter(); state.ParameterName = "@state"; oCmd.Parameters.Add(state);
                var postcode = oCmd.CreateParameter(); postcode.ParameterName = "@postcode"; oCmd.Parameters.Add(postcode);
                var email = oCmd.CreateParameter(); email.ParameterName = "@email"; oCmd.Parameters.Add(email);
                var phone = oCmd.CreateParameter(); phone.ParameterName = "@phone"; oCmd.Parameters.Add(phone);
                var status = oCmd.CreateParameter(); status.ParameterName = "@status"; oCmd.Parameters.Add(status);
                var created = oCmd.CreateParameter(); created.ParameterName = "@created"; oCmd.Parameters.Add(created);
                var lastupdate = oCmd.CreateParameter(); lastupdate.ParameterName = "@lastupdate"; oCmd.Parameters.Add(lastupdate);

                foreach (var order in salesOrders)
                {
                    id.Value = order.id;
                    orderRef.Value = order.orderRef;
                    parentId.Value = order.parentId;
                    order_key.Value = order.order_key;
                    order_number.Value = order.order_number;
                    customerId.Value = order.customerId;
                    customer_notes.Value = GetDataValue(order.customer_notes);
                    order_date.Value = GetDataValue(order.order_date);
                    date_created_gmt.Value = GetDataValue(order.date_created_gmt);
                    date_paid.Value = GetDataValue(order.date_paid);
                    date_completed.Value = GetDataValue(order.date_completed);
                    currency.Value = order.currency;
                    payment_menthod.Value = order.payment_menthod;
                    payment_menthod_title.Value = order.payment_menthod_title;
                    discount_total.Value = order.discount_total;
                    shipping_total.Value = GetDataValue(order.shipping_total);
                    prices_include_tax.Value = GetDataValue(order.prices_include_tax);
                    rateId.Value = GetDataValue(order.rateId);
                    rate_code.Value = GetDataValue(order.rate_code);
                    tax_rate_label.Value = GetDataValue(order.tax_rate_label);
                    shipping_tax.Value = GetDataValue(order.shipping_tax);
                    total_tax.Value = GetDataValue(order.total_tax);
                    discount_tax.Value = GetDataValue(order.discount_tax);
                    total.Value = GetDataValue(order.total);
                    first_name.Value = GetDataValue(order.first_name);
                    last_name.Value = GetDataValue(order.last_name);
                    company.Value = GetDataValue(order.company);
                    address1.Value = GetDataValue(order.address1);
                    address2.Value = GetDataValue(order.address2);
                    country.Value = GetDataValue(order.country);
                    city.Value = GetDataValue(order.city);
                    state.Value = GetDataValue(order.state);
                    postcode.Value = GetDataValue(order.postcode);
                    email.Value = GetDataValue(order.email);
                    phone.Value = GetDataValue(order.phone);
                    status.Value = GetDataValue(order.status);
                    created.Value = order.created;
                    lastupdate.Value = order.lastupdate;
                    oCmd.ExecuteNonQuery();
                }

                oCmd.Dispose();

                oConn.Close();
                oConn.Dispose();
            }
        }

        public static void OrderDetailBulkMerge(string connectionString, List<OrderDetails> orderDetails)
        {
            SqlConnection.ClearAllPools();
            using (var oConn = new SqlConnection(connectionString))
            {
                oConn.Open();


                var oCmd = new SqlCommand(@"MERGE orderdetails WITH(HOLDLOCK) as dest
                                        USING (VALUES (
                                            @id,
                                            @orderId,
                                            @productId,
                                            @descrip,
                                            @quantity,
                                            @price,
                                            @subtotal,
                                            @tax_class,
                                            @subtotal_tax,
                                            @total_tax,
                                            @total,
                                            @created,
                                            @lastupdate))
                                            AS src 
                                            (
                                            id,
                                            orderId,
                                            productId,
                                            descrip,
                                            quantity,
                                            price,
                                            subtotal,
                                            tax_class,
                                            subtotal_tax,
                                            total_tax,
                                            total,
                                            created,
                                            lastupdate
                                            )
                                            ON src.id = dest.id
                                        WHEN MATCHED THEN
                                            UPDATE SET 
	                                       [descrip]=src.descrip,
                                           [quantity]=src.quantity,
                                           [price]=src.price,
                                           [subtotal]=src.subtotal,
                                           [tax_class]=src.tax_class,
                                           [subtotal_tax]=src.subtotal_tax,
                                           [total_tax]=src.total_tax,
                                           [total]=src.total,
                                           [lastupdate]=src.lastupdate
                                            WHEN NOT MATCHED THEN
                                            INSERT VALUES 
		                                           (
                                                 src.[orderId],
                                                 src.[productId],
                                                 src.[descrip],
                                                 src.[quantity],
                                                 src.[price],
                                                 src.[subtotal],
                                                 src.[tax_class],
                                                 src.[subtotal_tax],
                                                 src.[total_tax],
                                                 src.[total],
                                                 src.[created],
                                                 src.[lastupdate]);",
                                                     oConn);

                          var id = oCmd.CreateParameter(); id.ParameterName = "@id"; oCmd.Parameters.Add(id);
                          var orderId = oCmd.CreateParameter(); orderId.ParameterName = "@orderId"; oCmd.Parameters.Add(orderId);
                          var productId = oCmd.CreateParameter(); productId.ParameterName = "@productId"; oCmd.Parameters.Add(productId);
                          var descrip = oCmd.CreateParameter(); descrip.ParameterName = "@descrip"; oCmd.Parameters.Add(descrip);
                          var quantity = oCmd.CreateParameter(); quantity.ParameterName = "@quantity"; oCmd.Parameters.Add(quantity);
                          var price = oCmd.CreateParameter(); price.ParameterName = "@price"; oCmd.Parameters.Add(price);
                          var subtotal = oCmd.CreateParameter(); subtotal.ParameterName = "@subtotal"; oCmd.Parameters.Add(subtotal);
                          var tax_class = oCmd.CreateParameter(); tax_class.ParameterName = "@tax_class"; oCmd.Parameters.Add(tax_class);
                          var subtotal_tax = oCmd.CreateParameter(); subtotal_tax.ParameterName = "@subtotal_tax"; oCmd.Parameters.Add(subtotal_tax);
                          var total_tax = oCmd.CreateParameter(); total_tax.ParameterName = "@total_tax"; oCmd.Parameters.Add(total_tax);
                          var total = oCmd.CreateParameter(); total.ParameterName = "@total"; oCmd.Parameters.Add(total);
                          var created = oCmd.CreateParameter(); created.ParameterName = "@created"; oCmd.Parameters.Add(created);
                          var lastupdate = oCmd.CreateParameter(); lastupdate.ParameterName = "@lastupdate"; oCmd.Parameters.Add(lastupdate);

                foreach (var item in orderDetails)
                {
                    id.Value = item.id;
                    orderId.Value = item.orderId;
                    productId.Value = item.productId;
                    descrip.Value = item.descrip;
                    quantity.Value = item.quantity;
                    price.Value = item.price;
                    subtotal.Value = item.subtotal;
                    tax_class.Value = item.tax_class;
                    subtotal_tax.Value = item.subtotal_tax;
                    total_tax.Value = item.total_tax;
                    total.Value = item.total;
                    created.Value = item.created;
                    lastupdate.Value = item.lastupdate;
                    oCmd.ExecuteNonQuery();
                }

                oCmd.Dispose();

                oConn.Close();
                oConn.Dispose();
            }
        }

        private static object GetDataValue(object value)
        {
            if (value == null)
            {
                return DBNull.Value;
            }

            return value;
        }


    }
}

