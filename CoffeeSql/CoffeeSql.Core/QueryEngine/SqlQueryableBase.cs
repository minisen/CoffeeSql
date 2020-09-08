using CoffeeSql.Core.DbContexts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CoffeeSql.Core.QueryEngine
{
    /// <summary>
    /// Related configuration information of SqlQueryable Class
    /// </summary>
    public abstract class SqlQueryableBase
    {
        public SqlQueryableBase(SqlDbContext _dbContext)
        {
            DbContext = _dbContext;
        }

        //context
        protected SqlDbContext DbContext;

        //paging
        protected bool _isPaging = false;
        protected int _pageIndex = 0;
        protected int _pageSize = 0;

        //query info
        public string SqlStatement { get; internal set; }
        public object[] Parameters { get; internal set; }

        //query result
        public abstract DataSet ToDataSet();
        public abstract object ToData();
        public abstract TEntity ToOne<TEntity>() where TEntity : class;
        public abstract List<TEntity> ToList<TEntity>() where TEntity : class;
    }
}
