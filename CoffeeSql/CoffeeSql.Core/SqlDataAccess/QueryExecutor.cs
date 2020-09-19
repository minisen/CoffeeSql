using CoffeeSql.Core.DbContexts;
using CoffeeSql.Core.SqlDataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeSql.Core.SqlDataAccess
{
    /// <summary>
    /// 查詢執行器
    ///     =>為了方便進行執行sql時切面化（AOP）統一控制，面向接口方式
    /// </summary>
    internal class QueryExecutor : IQueryExecute
    {

        #region ExecuteNonQuery 执行sql语句或者存储过程，返回影响的行数
        public int ExecuteNonQuery(SqlDbContext DbContext)
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                DbContext.ParameterInitializes();
                DbContext.ConnectionStatusCheck();
                return DbContext.DbCommand.ExecuteNonQuery();
            }

            return default(int);
        }

        public async Task<int> ExecuteNonQueryAsync(SqlDbContext DbContext)
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                DbContext.ParameterInitializes();
                DbContext.ConnectionStatusCheck();
                return await DbContext.DbCommand.ExecuteNonQueryAsync();
            }

            return default(int);
        }
        #endregion

        #region ExecuteScalar 执行sql语句或者存储过程,执行单条语句，返回单个结果
        public object ExecuteScalar(SqlDbContext DbContext)
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                DbContext.ParameterInitializes();
                DbContext.ConnectionStatusCheck();
                return DbContext.DbCommand.ExecuteScalar();
            }
            return default(object);
        }

        public async Task<object> ExecuteScalarAsync(SqlDbContext DbContext)
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                DbContext.ParameterInitializes();
                DbContext.ConnectionStatusCheck();
                return await DbContext.DbCommand.ExecuteScalarAsync();
            }
            return default(object);
        }
        #endregion

        #region ExecuteReader 执行sql语句或者存储过程,返回DataReader
        public DbDataReader ExecuteReader(SqlDbContext DbContext)
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                DbContext.ParameterInitializes();
                DbContext.ConnectionStatusCheck();
                return DbContext.DbCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }
            return default(DbDataReader);
        }
        public async Task<DbDataReader> ExecuteReaderAsync(SqlDbContext DbContext)
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                DbContext.ParameterInitializes();
                DbContext.ConnectionStatusCheck();
                return await DbContext.DbCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            }
            return default(DbDataReader);
        }
        #endregion

        #region ExecuteDataSet 执行sql语句或者存储过程,返回一个DataSet
        public DataSet ExecuteDataSet(SqlDbContext DbContext)
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                DbContext.ParameterInitializes();
                DbContext.ConnectionStatusCheck();
                DataSet ds = new DataSet();
                DbContext.DbDataAdapter.Fill(ds);
                return ds;
            }
            return default(DataSet);
        }

        public async Task<DataSet> ExecuteDataSetAsync(SqlDbContext DbContext)
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                var reader = await ExecuteReaderAsync(DbContext);
                DataSet ds = await DbDataReaderToDataSetAsync(reader);
                return ds;
            }
            return default(DataSet);
        }
        #endregion

        #region ExecuteDataTable 执行sql语句或者存储过程,返回一个DataTable
        public DataTable ExecuteDataTable(SqlDbContext DbContext)
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                DataSet ds = ExecuteDataSet(DbContext);
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    return ds.Tables[0];
                }
                return default(DataTable);
            }
            return default(DataTable);
        }

        public async Task<DataTable> ExecuteDataTableAsync(SqlDbContext DbContext)
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                DataSet ds = await ExecuteDataSetAsync(DbContext);
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    return ds.Tables[0];
                }
                return default(DataTable);
            }
            return default(DataTable);
        }
        #endregion

        #region ExecuteEntity 执行sql语句或者存储过程，返回一个Entity
        public Entity ExecuteEntity<Entity>(SqlDbContext DbContext) where Entity : class
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                DataSet ds = ExecuteDataSet(DbContext);
                return GetEntityFromDataSet<Entity>(ds);
            }
            return default(Entity);
        }

        public async Task<Entity> ExecuteEntityAsync<Entity>(SqlDbContext DbContext) where Entity : class
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                DataSet ds = await ExecuteDataSetAsync(DbContext);
                return GetEntityFromDataSet<Entity>(ds);
            }
            return default(Entity);
        }
        #endregion

        #region ExecuteList Entity 执行sql语句或者存储过程，返回一个List<T>
        public List<Entity> ExecuteList<Entity>(SqlDbContext DbContext) where Entity : class
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                DataSet ds = ExecuteDataSet(DbContext);
                return GetListFromDataSet<Entity>(ds);
            }
            return default(List<Entity>);
        }

        public async Task<List<Entity>> ExecuteListAsync<Entity>(SqlDbContext DbContext) where Entity : class
        {
            if (DbContext.OpenRealExecutionSaveToDb)
            {
                DataSet ds = await ExecuteDataSetAsync(DbContext);
                return GetListFromDataSet<Entity>(ds);
            }
            return default(List<Entity>);

        }
        #endregion

        #region ExpressionTree高性能转换DataSet为List集合
        public List<Entity> GetListFromDataSet<Entity>(DataSet ds) where Entity : class
        {
            DataTable dt = ds.Tables[0];
            if (dt.Rows.Count > 0)
            {
                List<Entity> list = new List<Entity>();
                foreach (DataRow row in dt.Rows)
                {
                    Entity entity = EntityFillAdapter<Entity>.AutoFill(row);
                    list.Add(entity);
                }
                return list;
            }
            return default(List<Entity>);
        }
        public Entity GetEntityFromDataSet<Entity>(DataSet ds) where Entity : class
        {
            DataTable dt = ds.Tables[0];// 获取到ds的dt
            if (dt.Rows.Count > 0)
            {
                return EntityFillAdapter<Entity>.AutoFill(dt.Rows[0]);
            }
            return default(Entity);
        }
        #endregion

        /// <summary>
        /// 將DbDataReader讀出到DataTable
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        private async Task<DataSet> DbDataReaderToDataSetAsync(DbDataReader dataReader)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            DataTable schemaTable = dataReader.GetSchemaTable();

            //动态构建表，添加列
            foreach (DataRow dr in schemaTable.Rows)
            {
                DataColumn dc = new DataColumn();
                //设置列的数据类型
                dc.DataType = dr[0].GetType();
                //设置列的名称
                dc.ColumnName = dr[0].ToString();
                //将该列添加进构造的表中
                dt.Columns.Add(dc);
            }
            //读取数据添加进表中
            while (await dataReader.ReadAsync())
            {
                DataRow row = dt.NewRow();
                //填充一行数据
                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {
                    row[i] = dataReader[i].ToString();

                }
                dt.Rows.Add(row);
                row = null;
            }
            dataReader.Close();
            schemaTable = null;
            ds.Tables.Add(dt);
            return ds;
        }
    }

}
