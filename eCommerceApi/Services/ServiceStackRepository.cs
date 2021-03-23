using ApiCore;
using ApiCore.Services;
using Microsoft.Extensions.Logging;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Z.Dapper.Plus;

namespace ApiCore.Services
{
    public class ServiceStackRepository<TEntity> : IRepository<TEntity> where TEntity : new()
    {

        private IDbConnectionFactory _dbFactory = null;

      
        public ServiceStackRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
          

        }



        public IEnumerable<TEntity> GetAll()
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                return db.Select<TEntity>();
            }



        }

        public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate)
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                return db.Select<TEntity>(predicate);
            }
        }


        public IEnumerable<TEntity> GetLoadRerefence(Expression<Func<TEntity, bool>> predicate)
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                return db.LoadSelect<TEntity>(predicate);
            }
        }

        public IEnumerable<TEntity> GetLoadRerefence()
        {
            using (var db = _dbFactory.OpenDbConnection())
            {

                return db.LoadSelect<TEntity>();
            }
        }


        public TEntity GetById(object id)
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                var value = Convert.ToInt32(id);
                return db.SingleById<TEntity>(value);
            }

        }

        public IEnumerable<TEntity> GetByIds(int[] ids)
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                return db.SelectByIds<TEntity>(ids);
            }
        }


        public int Insert(TEntity entity)
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                db.Insert<TEntity>(entity);
                return 1;

            }

        }

        public int Insert(object entity)
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                db.Insert(entity);
                return 1;

            }

        }


        public int Update(TEntity entity)
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                db.Update<TEntity>(entity);
                return 1;

            }
        }


        public int Update(TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                return db.UpdateNonDefaults<TEntity>(entity, predicate);
              

            }
        }


        public int UpdateAll(IEnumerable<TEntity> entities)
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
               return db.UpdateAll<TEntity>(entities);
        
            }
        }
        public int Delete(TEntity entity)
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                return db.Delete<TEntity>(entity);

            }
        }
        public int DeleteAll()
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                return db.DeleteAll<TEntity>();

            }
        }


        public object BulkInsert(IEnumerable<TEntity> entities)
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                var adoNetConn = ((IHasDbConnection)db).DbConnection;
                var sqlConnection = adoNetConn as SqlConnection;

                return sqlConnection.BulkInsert<TEntity>(entities);

            }

        }

        public object BulkMerge(IEnumerable<TEntity> entities)
        {
            using (var db = _dbFactory.OpenDbConnection())
            {
                var adoNetConn = ((IHasDbConnection)db).DbConnection;
                var sqlConnection = adoNetConn as SqlConnection;

                return sqlConnection.BulkMerge<TEntity>(entities);

            }

        }


    }
}
