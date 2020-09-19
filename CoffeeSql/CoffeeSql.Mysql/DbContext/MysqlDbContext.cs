using CoffeeSql.Core.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;
using CoffeeSql.Core;
using System.Data.Common;
using MySql.Data.MySqlClient;
using CoffeeSql.Core.SqlStatementManagement;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CoffeeSql.Core.EntityDesign.Attributes;
using System.Linq;
using CoffeeSql.Core.Extensions;
using CoffeeSql.Mysql.SqlStatementManagement;

namespace CoffeeSql.Mysql
{
    public abstract class MysqlDbContext<TDataBase> : SqlDbContext, IDisposable where TDataBase : class
    {
        protected MysqlDbContext(string connectionString_Write, params string[] connectionString_Read) : base(connectionString_Write, connectionString_Read)
        {
            this.DataBaseType = CoffeeSql.Core.Configs.DataBaseType.MySql;
            this.DataBaseName = DataBaseAttribute.GetName(typeof(TDataBase));
        }

        internal override DbConnection CreateDbConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        internal override DbCommand CreateDbCommand()
        {
            return new MySqlCommand();
        }

        internal override DbDataAdapter CreateDbDataAdpter()
        {
            DbDataAdapter newDbDataAdpter = new MySqlDataAdapter();
            newDbDataAdpter.SelectCommand = this.DbCommand;

            return newDbDataAdpter;
        }

        internal override void ParameterInitializes()
        {
            if (this.Parameters != null && this.Parameters.Any())
            {
                this.DbCommand.Parameters.Clear();
                this.Parameters.Foreach(t => DbCommand.Parameters.Add(new MySqlParameter(t.Key, t.Value ?? DBNull.Value)));
            }
        }
        
        internal override CommandTextGeneratorBase CreateCommandTextGenerator()
        {
            return new MysqlCommandTextGenerator(this);
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
        private class TableCacheDbContext : MysqlDbContext<TableCacheDbContext>
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
