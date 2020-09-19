using CoffeeSql.Core.DbContexts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace CoffeeSql.Core.SqlDataAccess.Interface
{
    /// <summary>
    /// interface of Database Query
    /// </summary>
    internal interface IQueryExecute
    {
        int ExecuteNonQuery(SqlDbContext DbContext);
        Task<int> ExecuteNonQueryAsync(SqlDbContext DbContext);
        object ExecuteScalar(SqlDbContext DbContext);
        Task<object> ExecuteScalarAsync(SqlDbContext DbContext);
        DbDataReader ExecuteReader(SqlDbContext DbContext);
        Task<DbDataReader> ExecuteReaderAsync(SqlDbContext DbContext);
        DataTable ExecuteDataTable(SqlDbContext DbContext);
        Task<DataTable> ExecuteDataTableAsync(SqlDbContext DbContext);
        DataSet ExecuteDataSet(SqlDbContext DbContext);
        Task<DataSet> ExecuteDataSetAsync(SqlDbContext DbContext);
        List<Entity> ExecuteList<Entity>(SqlDbContext DbContext) where Entity : class;
        Task<List<Entity>> ExecuteListAsync<Entity>(SqlDbContext DbContext) where Entity : class;
        Entity ExecuteEntity<Entity>(SqlDbContext DbContext) where Entity : class;
        Task<Entity> ExecuteEntityAsync<Entity>(SqlDbContext DbContext) where Entity : class;

    }
}
