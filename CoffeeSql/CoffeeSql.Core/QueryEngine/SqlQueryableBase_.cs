using CoffeeSql.Core.EntityDesign.Attributes;
using CoffeeSql.Core.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CoffeeSql.Core.QueryEngine
{
    /// <summary>
    /// SqlQueryable的相关配置信息
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class SqlQueryableBase<TEntity> where TEntity : class
    {
        public SqlQueryableBase(SqlDbContext _dbContext)
        {
            this.DbContext = _dbContext;
        }

        //context
        protected SqlDbContext DbContext;

        //where
        protected Expression<Func<TEntity, bool>> _where = t => true;

        //orderby
        protected Expression<Func<TEntity, object>> _orderby;
        protected bool _isDesc = false;

        //paging
        protected bool _isPaging = false;
        protected int _pageIndex = 0;
        protected int _pageSize = 0;

        /// <summary>
        /// 要查询的列
        /// </summary>
        protected Expression<Func<TEntity, object>> _columns;

        /// <summary>
        /// 必要条件检查
        /// </summary>
        protected void MustExistCheck()
        {
            if (_where == null)
            {
                throw new ArgumentNullException("Where condition deficiency");
            }
        }

        /// <summary>
        /// 获取TableName，并将其重新赋值
        /// </summary>
        protected void ReSetTableName()
        {
            DbContext.TableName = TableAttribute.GetName(typeof(TEntity));
        }

        /// <summary>
        /// 查询生成符合当前条件的List结果集
        /// </summary>
        /// <returns></returns>
        public abstract List<TEntity> ToList();
        /// <summary>
        /// 查询生成符合当前条件的单个实体
        /// </summary>
        /// <returns></returns>
        public abstract TEntity ToOne();
        /// <summary>
        /// 查询出符合当前条件的数据条数
        /// </summary>
        /// <returns></returns>
        public abstract long Count();
        /// <summary>
        /// 查询是否有符合条件的记录数
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public abstract bool Any();
    }
}
