﻿using CoffeeSql.Core.EntityDesign.Attributes;
using CoffeeSql.Core.DbContexts;
using CoffeeSql.Core.SqlStatementManagement;
using CoffeeSql.Oracle.SqlStatementManagement;
using CoffeeSql.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Concurrent;

namespace CoffeeSql.Oracle.DbContext
{
    public abstract class OracleDbContext<TDataBase> : SqlDbContext, IDisposable where TDataBase : class
    {
        protected OracleDbContext(string connectionString_Write, params string[] connectionString_Read) : base(connectionString_Write, connectionString_Read)
        {
            //add write connection to cache 
            GetDbConnection(connectionString_Write);

            this.DataBaseType = CoffeeSql.Core.Configs.DataBaseType.Oracle;
            this.DataBaseName = DataBaseAttribute.GetName(typeof(TDataBase));
        }

        internal override DbConnection CreateDbConnection(string connectionString)
        {
            return new OracleConnection(connectionString);
        }

        internal override DbCommand CreateDbCommand()
        {
            return new OracleCommand();
        }

        internal override DbDataAdapter CreateDbDataAdpter()
        {
            DbDataAdapter newDbDataAdpter = new OracleDataAdapter();
            newDbDataAdpter.SelectCommand = this.DbCommand;

            return newDbDataAdpter;
        }

        internal override void ParameterInitializes()
        {
            if (this.Parameters != null && this.Parameters.Any())
            {
                this.DbCommand.Parameters.Clear();
                this.Parameters.Foreach(t => DbCommand.Parameters.Add(new OracleParameter(t.Key, t.Value ?? DBNull.Value)));
            }
        }

        internal override CommandTextGeneratorBase CreateCommandTextGenerator()
        {
            return new OracleCommandTextGenerator(this);
        }
        
        internal override List<TEntity> GetFullCollectionData<TEntity>()
        {
            //多线程下使用同一个Connection会出现问题，这里采用新的连接进行异步数据操作
            using (var db = new TableCacheDbContext(this.ConnectionManager.ConnectionString_Write, this.ConnectionManager.ConnectionStrings_Read))
            {
                db.DataBaseName = this.DataBaseName;
                db.SqlStatement = $"SELECT * FROM {CollectionName}";
                return db.QueryExecutor.ExecuteList<TEntity>(db);
            }
        }

        /// <summary>
        /// 专门为扫描数据库表做的上下文
        /// </summary>
        private class TableCacheDbContext : OracleDbContext<TableCacheDbContext>
        {
            public TableCacheDbContext(string connectionString_Write, params string[] connectionStrings_Read) : base(connectionString_Write, connectionStrings_Read)
            {
            }
        }

        public new void Dispose()
        {
            base.Dispose();
        }
    }
}
