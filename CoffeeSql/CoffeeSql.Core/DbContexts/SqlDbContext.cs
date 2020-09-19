using CoffeeSql.Core.Validation;
using CoffeeSql.Core.Configs;
using CoffeeSql.Core.ExternalInterface;
using CoffeeSql.Core.SqlDataAccess.Interface;
using CoffeeSql.Core.SqlDataAccess;
using CoffeeSql.Core.Transaction;
using CoffeeSql.Core.SqlStatementManagement;
using CoffeeSql.Core.EntityDesign.Attributes;
using CoffeeSql.Core.QueryEngine;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Runtime.CompilerServices;
using Castle.DynamicProxy;
using CoffeeSql.Core.AopInterceptors;
using System.Linq.Expressions;
using System.Linq;
using CoffeeSql.Core.Helpers;
using CoffeeSql.Core.CodeFirst;
using System.Threading.Tasks;

//The type to be extended needs to add the corresponding assembly friend ID here
[assembly: InternalsVisibleTo("CoffeeSql.Mysql")]
[assembly: InternalsVisibleTo("CoffeeSql.Oracle")]
[assembly: InternalsVisibleTo("CoffeeSql.Oracle.Core")]
namespace CoffeeSql.Core.DbContexts
{
    /// <summary>
    /// Sql Database Operation Context
    /// </summary>
    public abstract class SqlDbContext : DbContext, IExecuteSqlOperate
    {
        private static ProxyGenerator proxyCreator;

        static SqlDbContext()
        {
            proxyCreator = new ProxyGenerator();
        }

        protected SqlDbContext(string connectionString_Write, params string[] connectionString_Read) : base(connectionString_Write, connectionString_Read)
        {
            ConnectionManager.SetConnectionString(OperationType.Write);    //Initialize connection string
            this.DbConnection = GetDbConnection(ConnectionManager.CurrentConnectionString);
            this.DbCommand = CreateDbCommand();
            this.DbDataAdapter = CreateDbDataAdpter();
            AccessorInitializes();
            this.CommandTextGenerator = CreateCommandTextGenerator();
            DBTransaction = new DBTransaction(this);
            this.Parameters = new Dictionary<string, object>();

            //Get QueryExecutor's Proxy
            QueryExecutor = proxyCreator.CreateInterfaceProxyWithTarget<IQueryExecute>(new QueryExecutor(), new DBLogInterceptor()); //new QueryExecutor();
        }

        #region Database Control

        /// <summary>
        /// Table Name
        /// </summary>
        public string TableName
        {
            get
            {
                return CollectionName;
            }

            internal set
            {
                CollectionName = value;
            }
        }

        /// <summary>
        /// Sql Statement，Gets or assigns the Command Text parameter of a command line object 
        /// </summary>
        public string SqlStatement
        {
            get
            {
                return this.DbCommand?.CommandText;
            }

            internal set
            {
                if (this.DbCommand == null)
                {
                    throw new NullReferenceException("DbCommand is null,please initialize command first!");
                }
                this.DbCommand.CommandText = value;
            }
        }

        /// <summary>
        /// Parameterized query parameters
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Database connect Manager
        /// </summary>
        internal DbConnection DbConnection { get; set; }

        /// <summary>
        /// Sql Command Manager
        /// </summary>
        internal DbCommand DbCommand { get; set; }

        /// <summary>
        /// Database Data Adapter
        /// </summary>
        internal DbDataAdapter DbDataAdapter { get; set; }

        /// <summary>
        /// Command Text Generator
        /// </summary>
        internal CommandTextGeneratorBase CommandTextGenerator { get; set; }

        /// <summary>
        /// Get Database Connection
        /// </summary>
        internal DbConnection GetDbConnection(string connectionString)
        {
            return CreateDbConnection(connectionString);
        }

        /// <summary>
        /// Create Database Command
        /// </summary>
        internal abstract DbCommand CreateDbCommand();

        /// <summary>
        /// Create Database DataAdpter
        /// </summary>
        internal abstract DbDataAdapter CreateDbDataAdpter();

        /// <summary>
        /// Create CommandText Generator
        /// </summary>
        internal abstract CommandTextGeneratorBase CreateCommandTextGenerator();

        /// <summary>
        /// Connection status check, if closed, open connection
        /// </summary>
        internal void ConnectionStatusCheck()
        {
            if (DbConnection.State != ConnectionState.Open)
            {
                DbConnection.Open();
            }
        }

        /// <summary>
        /// Initialize Accessor
        /// </summary>
        internal void AccessorInitializes()
        {
            //Set the property value of SqlCommand object
            DbCommand.CommandTimeout = CoffeeSqlConst.CommandTimeout;
        }

        /// <summary>
        /// Initialize Parameter
        /// </summary>
        internal abstract void ParameterInitializes();

        /// <summary>
        /// DB Query Executor
        /// </summary>
        internal IQueryExecute QueryExecutor { get; private set; }

        /// <summary>
        /// Get table name from TEntity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public string GetTableName<TEntity>() where TEntity : class
            => TableAttribute.GetName(typeof(TEntity));

        /// <summary>
        /// 获取一级缓存的缓存键
        /// </summary>
        /// <returns></returns>
        internal override string GetQueryCacheKey()
        {
            //如果有条件，则sql的key要拼接对应的参数值
            if (Parameters != null && Parameters.Any())
            {
                string s = $"{SqlStatement}_{string.Join("|", Parameters.Values)}";
                return MD5Helper.GetMd5Hash(s);
            }
            return MD5Helper.GetMd5Hash(SqlStatement);
        }

        /// <summary>
        /// Database Log Action
        /// </summary>
        public Action<SqlDbContext> Log { get; set; }

        #endregion

        #region Transaction Handle

        /// <summary>
        /// Transaction Manager
        /// </summary>
        public DBTransaction DBTransaction { get; private set; }

        #endregion

        #region Strongly Typed Execution API

        public override int Add<TEntity>(TEntity entity)
        {
            PropertyDataValidator.Verify<TEntity>(this, entity);

            this.Parameters.Clear();
            this.ConnectionManager.SetConnectionString(OperationType.Write);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;
            this.DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Add(entity);
            this.TableName = GetTableName<TEntity>();

            this.DBTransaction.SyncCommandTransaction();

            int res = this.QueryExecutor.ExecuteNonQuery(this);
            DbCacheManager.Add(entity);
            return res;
        }
        public override int Update<TEntity>(TEntity entity)
        {
            Expression<Func<TEntity, bool>> filter;

            this.Parameters.Clear();
            this.ConnectionManager.SetConnectionString(OperationType.Write);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;
            this.DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Update(entity, out filter);
            this.TableName = GetTableName<TEntity>();

            this.DBTransaction.SyncCommandTransaction();

            int res = this.QueryExecutor.ExecuteNonQuery(this);
            DbCacheManager.Update(entity, filter);
            return res;
        }
        public override int Delete<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            this.Parameters.Clear();
            this.ConnectionManager.SetConnectionString(OperationType.Write);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;
            this.DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Delete(filter);
            this.TableName = GetTableName<TEntity>();

            this.DBTransaction.SyncCommandTransaction();

            int res = this.QueryExecutor.ExecuteNonQuery(this);
            DbCacheManager.Delete(filter);
            return res;
        }
        public override int Delete<TEntity>(TEntity entity)
        {
            this.Parameters.Clear();
            this.ConnectionManager.SetConnectionString(OperationType.Write);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;
            this.DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Delete(entity);
            this.TableName = GetTableName<TEntity>();

            this.DBTransaction.SyncCommandTransaction();

            int res = this.QueryExecutor.ExecuteNonQuery(this);
            DbCacheManager.Delete(entity);
            return res;
        }

        public override async Task<int> AddAsync<TEntity>(TEntity entity)
        {
            PropertyDataValidator.Verify<TEntity>(this, entity);

            this.Parameters.Clear();
            this.ConnectionManager.SetConnectionString(OperationType.Write);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;
            this.DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Add(entity);
            this.TableName = GetTableName<TEntity>();

            this.DBTransaction.SyncCommandTransaction();

            int res = await this.QueryExecutor.ExecuteNonQueryAsync(this);
            DbCacheManager.Add(entity);
            return res;
        }
        public override async Task<int> UpdateAsync<TEntity>(TEntity entity)
        {
            Expression<Func<TEntity, bool>> filter;

            this.Parameters.Clear();
            this.ConnectionManager.SetConnectionString(OperationType.Write);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;
            this.DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Update(entity, out filter);
            this.TableName = GetTableName<TEntity>();

            this.DBTransaction.SyncCommandTransaction();

            int res = await this.QueryExecutor.ExecuteNonQueryAsync(this);
            DbCacheManager.Update(entity, filter);
            return res;
        }
        public override async Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            this.Parameters.Clear();
            this.ConnectionManager.SetConnectionString(OperationType.Write);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;
            this.DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Delete(filter);
            this.TableName = GetTableName<TEntity>();

            this.DBTransaction.SyncCommandTransaction();

            int res = await this.QueryExecutor.ExecuteNonQueryAsync(this);
            DbCacheManager.Delete(filter);
            return res;
        }
        public override async Task<int> DeleteAsync<TEntity>(TEntity entity)
        {
            this.Parameters.Clear();
            this.ConnectionManager.SetConnectionString(OperationType.Write);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;
            this.DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Delete(entity);
            this.TableName = GetTableName<TEntity>();

            this.DBTransaction.SyncCommandTransaction();

            int res = await this.QueryExecutor.ExecuteNonQueryAsync(this);
            DbCacheManager.Delete(entity);
            return res;
        }

        public UpdateNonQueryable<TEntity> Update<TEntity>(Expression<Func<TEntity, object>> columns, TEntity entity) where TEntity : class
        {
            //Reset the command builder to prevent the last query parameter from being reused
            this.CommandTextGenerator = CreateCommandTextGenerator();

            this.Parameters.Clear();
            this.ConnectionManager.SetConnectionString(OperationType.Write);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;
            this.DbCommand.CommandType = CommandType.Text;
            this.TableName = GetTableName<TEntity>();


            this.DBTransaction.SyncCommandTransaction();

            return new UpdateNonQueryable<TEntity>(this).Set(columns, entity);

        }

        #endregion

        #region Weak Typed Execution API

        public int ExecuteSql(string sqlStatement, params object[] parms)
        {
            //Reset the command builder to prevent the last query parameter from being reused
            this.CommandTextGenerator = CreateCommandTextGenerator();
            this.CommandTextGenerator.SetSql_Params(sqlStatement, parms);
            this.CommandTextGenerator.QueryableQuery();

            //Initialize context parameters
            this.ConnectionManager.SetConnectionString(OperationType.Write);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;
            this.DbCommand.CommandType = CommandType.Text;

            this.DBTransaction.SyncCommandTransaction();

            return this.QueryExecutor.ExecuteNonQuery(this);
        }

        public async Task<int> ExecuteSqlAsync(string sqlStatement, params object[] parms)
        {
            //Reset the command builder to prevent the last query parameter from being reused
            this.CommandTextGenerator = CreateCommandTextGenerator();
            this.CommandTextGenerator.SetSql_Params(sqlStatement, parms);
            this.CommandTextGenerator.QueryableQuery();

            //Initialize context parameters
            this.ConnectionManager.SetConnectionString(OperationType.Write);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;
            this.DbCommand.CommandType = CommandType.Text;

            this.DBTransaction.SyncCommandTransaction();

            return await this.QueryExecutor.ExecuteNonQueryAsync(this);
        }

        public StoredProcedureNonQueryable StoredProcedureNonQueryable(string storeProcedureName, params DbParameter[] dbParams)
        {
            //Reset the command builder to prevent the last query parameter from being reused
            this.CommandTextGenerator = CreateCommandTextGenerator();

            //Clear the parameterized query parameters and keep the set of parameterized query parameters empty
            this.Parameters.Clear();

            //Initialize context parameters
            this.DbCommand.CommandText = storeProcedureName;
            this.DbCommand.CommandType = CommandType.StoredProcedure;
            this.DbCommand.Parameters.AddRange(dbParams);
            this.ConnectionManager.SetConnectionString(OperationType.Write);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;

            return new StoredProcedureNonQueryable(this) { StoredProcedureName = storeProcedureName, DBParameters = dbParams };
        }

        #endregion

        #region Query API

        /// <summary>
        /// SQL Strongly Type Complex Querier
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public SqlQueryable<TEntity> Queryable<TEntity>() where TEntity : class
        {
            //Reset the command builder to prevent the last query parameter from being reused
            this.CommandTextGenerator = CreateCommandTextGenerator();

            //Initialize context parameters
            this.Parameters.Clear();
            this.DbCommand.CommandType = CommandType.Text;
            this.ConnectionManager.SetConnectionString(OperationType.Read);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;
            this.TableName = GetTableName<TEntity>();


            return new SqlQueryable<TEntity>(this);
        }

        /// <summary>
        /// SQL Weak Type Complex Querier
        /// </summary>
        /// <param name="sqlStatement">SQL query statement, use {0}, {1}... To occupy query parameters</param>
        /// <param name="parms">Parameterized query parameter value set</param>
        /// <returns></returns>
        public SqlQueryable Queryable(string sqlStatement, params object[] parms)
        {
            //Reset the command builder to prevent the last query parameter from being reused
            this.CommandTextGenerator = CreateCommandTextGenerator();
            this.CommandTextGenerator.SetSql_Params(sqlStatement, parms);

            //Initialize context parameters
            this.Parameters.Clear();
            this.DbCommand.CommandType = CommandType.Text;
            this.ConnectionManager.SetConnectionString(OperationType.Read);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;


            return new SqlQueryable(this) { SqlStatement = sqlStatement, Parameters = parms };
        }

        /// <summary>
        /// Storedprocedure Weak Type Complex Querier
        /// </summary>
        /// <param name="storeProcedureName"></param>
        /// <param name="dbParams"></param>
        /// <returns></returns>
        public StoredProcedureQueryable StoredProcedureQueryable(string storeProcedureName, params DbParameter[] dbParams)
        {
            //Reset the command builder to prevent the last query parameter from being reused
            this.CommandTextGenerator = CreateCommandTextGenerator();

            //Clear the parameterized query parameters and keep the set of parameterized query parameters empty
            this.Parameters.Clear();

            //Initialize context parameters
            this.DbCommand.CommandText = storeProcedureName;
            this.DbCommand.CommandType = CommandType.StoredProcedure;
            this.DbCommand.Parameters.AddRange(dbParams);
            this.ConnectionManager.SetConnectionString(OperationType.Write);
            this.DbConnection = this.GetDbConnection(this.ConnectionManager.CurrentConnectionString);
            this.DbCommand.Connection = this.DbConnection;

            return new StoredProcedureQueryable(this) { StoredProcedureName = storeProcedureName, DBParameters = dbParams };
        }

        #endregion

        public new void Dispose()
        {
            //Release unmanaged resources
            if (this.DbDataAdapter != null)
                this.DbDataAdapter.Dispose();

            if (this.DbCommand != null)
                this.DbCommand.Dispose();

            if (this.DBTransaction != null)
                this.DBTransaction.Dispose();

            if (this.DbConnection.State == ConnectionState.Open)
                this.DbConnection.Close();
            if (this.DbConnection != null)
                this.DbConnection.Dispose();  

            base.Dispose();
        }

    }
}
