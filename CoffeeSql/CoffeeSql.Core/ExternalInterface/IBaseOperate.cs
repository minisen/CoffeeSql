using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeSql.Core.ExternalInterface
{
    /// <summary>
    /// The Api of Base Operation
    /// </summary>
    public interface IBaseOperate
    {
        int Add<TEntity>(TEntity entity) where TEntity : class;
        //Task AddAsync<TEntity>(TEntity entity) where TEntity : class;

        //int Update<TEntity>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>> columns, TEntity entity) where TEntity : class;
        //Task UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> filter, TEntity entity) where TEntity : class;

        int Delete<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;
        //Task DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;
    }
}
