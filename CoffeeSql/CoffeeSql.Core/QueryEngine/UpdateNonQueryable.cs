using CoffeeSql.Core.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CoffeeSql.Core.QueryEngine
{
    public class UpdateNonQueryable<TEntity> where TEntity : class
    {
        public UpdateNonQueryable(SqlDbContext _dbContext)
        {
            this.DbContext = _dbContext;
        }

        //context
        protected SqlDbContext DbContext;

        protected Expression<Func<TEntity, object>> _columns;
        protected Expression<Func<TEntity, bool>> _filter;
        protected TEntity _entity;

        public UpdateNonQueryable<TEntity> Set(Expression<Func<TEntity, object>> columns, TEntity entity)
        {
            this._columns = columns;
            this._entity = entity;

            return this;
        }

        public UpdateNonQueryable<TEntity> Where(Expression<Func<TEntity, bool>> filter)
        {
            this._filter = filter;

            return this;
        }

        public int Done()
        {
            this.DbContext.CommandTextGenerator.SetColumns(_columns);
            this.DbContext.CommandTextGenerator.Update(_filter, _entity);
            
            int res = this.DbContext.QueryExecutor.ExecuteNonQuery(this.DbContext);
            this.DbContext.DbCacheManager.Update(_entity, _filter, this.DbContext.CommandTextGenerator._columns);
            return res;
        }
    }
}
