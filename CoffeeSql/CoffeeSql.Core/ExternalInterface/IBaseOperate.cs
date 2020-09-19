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
        Task<int> AddAsync<TEntity>(TEntity entity) where TEntity : class;

        int Update<TEntity>(TEntity entity) where TEntity : class;
        Task<int> UpdateAsync<TEntity>(TEntity entity) where TEntity : class;

        int Delete<TEntity>(TEntity entity) where TEntity : class;
        Task<int> DeleteAsync<TEntity>(TEntity entity) where TEntity : class;
        int Delete<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;
        Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;
    }
}
