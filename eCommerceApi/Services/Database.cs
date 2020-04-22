using ApiCore.Services;
using eCommerceApi.Model;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using System;

namespace eCommerceApi.DAL.Services
{
    public class Database
    {
        private IRepository<Customers> _Customers = null;
        private IRepository<ProductCategories> _ProductsCategories = null;
        private IRepository<Products> _Products = null;
        private IRepository<Orders> _salesOrders = null;
        private IRepository<OrderDetails> _salesOrderssDetails = null;

        private string _connectionString;
        private IDbConnectionFactory _dbFactory;



        public Database(string connectionString)
        {


            this._connectionString = connectionString;
            _dbFactory = new OrmLiteConnectionFactory(_connectionString, SqlServer2014Dialect.Provider);

        }

        public IRepository<Customers> Customers
        {
            get
            {
                if (_Customers == null)
                    _Customers = new ServiceStackRepository<Customers>(_dbFactory);

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

        public IRepository<Products> Products
        {
            get
            {
                if (_Products == null)
                    _Products = new ServiceStackRepository<Products>(_dbFactory);

                return _Products;
            }

        }
      
        public IRepository<Orders> Orders
        {
            get
            {
                if (_salesOrders == null)
                    _salesOrders = new ServiceStackRepository<Orders>(_dbFactory);

                return _salesOrders;
            }
        }


        public IRepository<OrderDetails> OrderDetails
        {
            get
            {
                if (_salesOrderssDetails == null)
                    _salesOrderssDetails = new ServiceStackRepository<OrderDetails>(_dbFactory);

                return _salesOrderssDetails;
            }
        }


   



      
    }
}
