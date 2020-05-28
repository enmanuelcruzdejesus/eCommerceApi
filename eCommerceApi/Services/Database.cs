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
        private IRepository<SyncTables> _syncTables = null;
        private IRepository<TransactionSyncLog> _transSyncLog = null;

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

        public IRepository<SyncTables> SyncTables
        {
            get
            {
                if (_syncTables == null)
                    _syncTables = new ServiceStackRepository<SyncTables>(_dbFactory);

                return _syncTables;
            }
        }

        public IRepository<TransactionSyncLog> TransSyncLog
        {
            get
            {
                if (_transSyncLog == null)
                    _transSyncLog = new ServiceStackRepository<TransactionSyncLog>(_dbFactory);

                return _transSyncLog;
            }
        }


        public DateTime GetLastUpdateDate(int userId, string tableName)
        {
            using (var dbCmd = _dbFactory.Open().CreateCommand())
            {
                dbCmd.CommandText = String.Format("SELECT LastUpdateSync FROM synctables WHERE UserId = {0} AND TableName = '{1}'", userId, tableName);
                var result = dbCmd.ExecuteScalar();
                if (result == DBNull.Value) return DateTime.MinValue;
                return (DateTime)result;
            }
        }









    }
}
