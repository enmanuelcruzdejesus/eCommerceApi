using eCommerceApi.DAL.Services.Services;
using eCommerceApi.Model;
using System;
using XamCore.Services;

namespace eCommerceApi.DAL.Services
{
    public class Database
    {
        private IRepository<Customer> _Customers = null;
        private IRepository<ProductCategory> _ProductsCategories = null;
        private IRepository<Product> _Products = null;
        private IRepository<Order> _salesOrders = null;
        private IRepository<OrderDetail> _salesOrderssDetails = null;
 

        private string _connectionString;
        private DatabaseContext _context;


        public Database(string connectionString)
        {

            this._connectionString = connectionString;
            this._context = new DatabaseContext(connectionString);
            

        }

        public IRepository<Customer> Customers
        {
            get
            {
                if (_Customers == null)
                    _Customers = new EntityFrameworkRepo<Customer>(_context);

                return _Customers;
            }

        }

        public IRepository<ProductCategory> ProductCategories
        {
            get
            {
                if (_ProductsCategories == null)
                    _ProductsCategories = new EntityFrameworkRepo<ProductCategory>(_context);

                return _ProductsCategories;
            }

        }

        public IRepository<Product> Products
        {
            get
            {
                if (_Products == null)
                    _Products = new EntityFrameworkRepo<Product>(_context);

                return _Products;
            }

        }
      
        public IRepository<Order> Orders
        {
            get
            {
                if (_salesOrders == null)
                    _salesOrders = new EntityFrameworkRepo<Order>(_context);

                return _salesOrders;
            }
        }


        public IRepository<OrderDetail> OrderDetails
        {
            get
            {
                if (_salesOrderssDetails == null)
                    _salesOrderssDetails = new EntityFrameworkRepo<OrderDetail>(_context);

                return _salesOrderssDetails;
            }
        }


   



      
    }
}
