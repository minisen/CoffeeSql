using CoffeeSql.Core.DbContexts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeSql.Core.QueryEngine
{
    public class StoredProcedureNonQueryable
    {
        public StoredProcedureNonQueryable(SqlDbContext _dbContext)
        {
            DbContext = _dbContext;
        }

        //context
        protected SqlDbContext DbContext;

        //query info
        public string StoredProcedureName { get; internal set; }
        public DbParameter[] DBParameters { get; internal set; }

        public void Done()
        {
            DbContext.QueryExecutor.ExecuteNonQuery(DbContext);
        }
    }
}
