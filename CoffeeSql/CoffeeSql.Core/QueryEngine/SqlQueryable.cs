using CoffeeSql.Core.DbContexts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CoffeeSql.Core.QueryEngine
{
    /// <summary>
    /// Strongly Typed Execution API
    /// </summary>
    public class SqlQueryable : SqlQueryableBase
    {
        public SqlQueryable(SqlDbContext _dbContext) : base(_dbContext)
        {
            DbContext.DbCommand.CommandType = CommandType.Text;
        }

        public SqlQueryable Paging(int pageIndex, int pageSize)
        {
            _isPaging = true;

            if (pageIndex <= 0)
                pageIndex = 0;

            if (pageSize <= 0)
                pageSize = 10;

            _pageIndex = pageIndex;
            _pageSize = pageSize;
            return this;
        }

        public override DataSet ToDataSet()
        {
            if (_isPaging)
            {
                DbContext.CommandTextGenerator.SetPage(_pageIndex, _pageSize);
                DbContext.CommandTextGenerator.QueryablePaging();
            }
            else
            {
                DbContext.CommandTextGenerator.QueryableQuery();
            }

            return DbContext.QueryExecutor.ExecuteDataSet(DbContext);
        }
        public override object ToData()
        {
            DbContext.CommandTextGenerator.QueryableQuery();

            return DbContext.QueryExecutor.ExecuteScalar(DbContext);
        }
        public override TEntity ToOne<TEntity>()
        {
            DbContext.CommandTextGenerator.QueryableQuery();

            return DbContext.QueryExecutor.ExecuteEntity<TEntity>(DbContext);
        }
        public override List<TEntity> ToList<TEntity>()
        {
            if (_isPaging)
            {
                DbContext.CommandTextGenerator.SetPage(_pageIndex, _pageSize);
                DbContext.CommandTextGenerator.QueryablePaging();
            }
            else
            {
                DbContext.CommandTextGenerator.QueryableQuery();
            }

            return DbContext.QueryExecutor.ExecuteList<TEntity>(DbContext);
        }
    }
}
