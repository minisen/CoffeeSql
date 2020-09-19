using CoffeeSql.Core.DbContexts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeSql.Core.QueryEngine
{
    public class StoredProcedureQueryable
    {
        public StoredProcedureQueryable(SqlDbContext _dbContext)
        {
            DbContext = _dbContext;
        }

        //context
        protected SqlDbContext DbContext;

        //query info
        public string StoredProcedureName { get; internal set; }
        public DbParameter[] DBParameters { get; internal set; }

        public DataSet ToDataSet()
        {
            return DbContext.QueryExecutor.ExecuteDataSet(DbContext);
        }
        public DataTable ToDataTable()
        {
            return DbContext.QueryExecutor.ExecuteDataTable(DbContext);
        }
        public List<TEntity> ToList<TEntity>() where TEntity : class
        {
            return DbContext.QueryExecutor.ExecuteList<TEntity>(DbContext);
        }

        public async Task<DataSet> ToDataSetAsync()
        {
            return await DbContext.QueryExecutor.ExecuteDataSetAsync(DbContext);
        }
        public async Task<DataTable> ToDataTableAsync()
        {
            return await DbContext.QueryExecutor.ExecuteDataTableAsync(DbContext);
        }
        public async Task<List<TEntity>> ToListAsync<TEntity>() where TEntity : class
        {
            return await DbContext.QueryExecutor.ExecuteListAsync<TEntity>(DbContext);
        }
        
    }
}
