
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ApiCore.Services
{
    public interface IRepository<TEntity> where TEntity : new()
    {
        IEnumerable<TEntity> GetAll();

        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate);

        IEnumerable<TEntity> GetLoadRerefence();


        IEnumerable<TEntity> GetLoadRerefence(Expression<Func<TEntity, bool>> predicate);



        TEntity GetById(object id);

        IEnumerable<TEntity> GetByIds(int[] ids);


        int Update(TEntity entity);

        int Update(TEntity entity, Expression<Func<TEntity, bool>> predicate);

        int Insert(TEntity entity);

        int Insert(object entity);


        int Delete(TEntity entity);

        int DeleteAll();





        object BulkInsert(IEnumerable<TEntity> entities);

        object BulkMerge(IEnumerable<TEntity> entities);









    }
}
