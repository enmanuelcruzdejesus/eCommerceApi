using ApiCore.Services;
using eCommerceApi.Model;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using System;

namespace eCommerceApi.DAL.Services
{
    public class Database
    {
        private IRepository<Customer> _Customers = null;
        private IRepository<ProductCategories> _ProductsCategories = null;
        private IRepository<Product> _Products = null;
        private IRepository<Order> _salesOrders = null;
        private IRepository<OrderDetail> _salesOrderssDetails = null;

        private string _connectionString;
        private IDbConnectionFactory _dbFactory;



        public Database(string connectionString)
        {


            this._connectionString = connectionString;
            _dbFactory = new OrmLiteConnectionFactory(_connectionString, SqlServer2014Dialect.Provider);

        }

        public IRepository<Customer> Customers
        {
            get
            {
                if (_Customers == null)
                    _Customers = new ServiceStackRepository<Customer>(_dbFactory);

                return _Customers;
            }

        }

        public IRepository<ProductCategories> ProductCategories
        {
            get
            {
                if (_ProductsCategories == null)
                    _ProductsCategories = new ServiceStackRepository<ProductCategories>(_dbFactory);

                return _ProductsCategories;
            }

        }

        public IRepository<Product> Products
        {
            get
            {
                if (_Products == null)
                    _Products = new ServiceStackRepository<Product>(_dbFactory);

                return _Products;
            }

        }
      
        public IRepository<Order> Orders
        {
            get
            {
                if (_salesOrders == null)
                    _salesOrders = new ServiceStackRepository<Order>(_dbFactory);

                return _salesOrders;
            }
        }


        public IRepository<OrderDetail> OrderDetails
        {
            get
            {
                if (_salesOrderssDetails == null)
                    _salesOrderssDetails = new ServiceStackRepository<OrderDetail>(_dbFactory);

                return _salesOrderssDetails;
            }
        }


   



      
    }
}
