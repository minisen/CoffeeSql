using CoffeeSql.Core.DbContexts;
using CoffeeSql.Core.SqlStatementManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeSql.Core.Extensions;
using System.Linq.Expressions;
using CoffeeSql.Core.EntityDesign.Attributes;
using System.Reflection;
using CoffeeSql.Core.Exceptions;

namespace CoffeeSql.Oracle.SqlStatementManagement
{
    internal class OracleCommandTextGenerator : CommandTextGeneratorBase
    {
        private static string tableAlias1 = tableAlias + "1";
        private static string tableAlias2 = tableAlias + "2";

        public OracleCommandTextGenerator(SqlDbContext _dbContext) : base(_dbContext) { }

        protected override string ParametricFlag { get => ":"; }

        protected override string FormatPagingSQL(string originalSQL)
        {
            return $"SELECT * FROM (SELECT {tableAlias2}.*, ROWNUM AS rowno FROM ({ originalSQL}) {tableAlias2} WHERE ROWNUM <= {_pageIndex * _pageSize}) { tableAlias1} WHERE { tableAlias1}.rowno >= {(_pageIndex - 1) * _pageSize + 1}".TrimEnd();
        }

        public override void SetPage(int pageIndex, int pageSize)
        {
            this._pageIndex = pageIndex;
            this._pageSize = pageSize;
        }
        public override void SetSql_Params(string sqlStatement, object[] param)
        {
            this._mainSqlStatement = sqlStatement;
            this._params = param;
        }

        public override void SetColumns<TEntity>(Expression<Func<TEntity, object>> columns)
        {
            _columns = LambdaToSql.ConvertColumns<TEntity>(columns);
        }
        public override void SetWhere<TEntity>(Expression<Func<TEntity, bool>> where)
        {
            this._where = LambdaToSql.ConvertWhere(where, DbContext.Parameters);
        }
        public override void SetOrderBy<TEntity>(Expression<Func<TEntity, object>> orderBy, bool isDesc)
        {
            if (orderBy == null)
                return;
            string desc = isDesc ? "DESC" : "ASC";
            this._orderBy = $" ORDER BY {LambdaToSql.ConvertOrderBy(orderBy)} {desc}".TrimEnd();
        }

        public override void SetLimit(int count)
        {
            _limit = $"AND ROWNUM <= {count}";
        }

        public override void SetAlias(string alias)
        {
            this._alias = alias;
        }

        public override string Add<TEntity>(TEntity entity)
        {
            DbContext.TableName = TableAttribute.GetName(typeof(TEntity));
            DbContext.Parameters = new Dictionary<string, object>();

            StringBuilder builder_front = new StringBuilder(), builder_behind = new StringBuilder();
            builder_front.Append("INSERT INTO ");
            builder_front.Append(DbContext.TableName);
            builder_front.Append(" (");
            builder_behind.Append(" VALUES (");

            PropertyInfo[] propertyInfos = GetPropertiesDicByType(typeof(TEntity));
            string columnName = string.Empty;
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                //AutoIncrease : if property is auto increase attribute skip this column.
                if (propertyInfo.GetCustomAttribute(typeof(AutoIncreaseBaseAttribute), true) is AutoIncreaseBaseAttribute autoIncreaseAttr)
                {
                }
                //Column
                else if (propertyInfo.GetCustomAttribute(typeof(ColumnBaseAttribute), true) is ColumnBaseAttribute column)
                {
                    builder_front.Append(column.GetName(propertyInfo.Name));
                    builder_front.Append(",");
                    builder_behind.Append(ParametricFlag);
                    columnName = column.GetName(propertyInfo.Name).Replace("`", "");
                    builder_behind.Append(columnName);
                    builder_behind.Append(",");

                    DbContext.Parameters.AddOrUpdate($"{ParametricFlag}{columnName}", propertyInfo.GetValue(entity));
                }

                //in the end,remove the redundant symbol of ','
                if (propertyInfos.Last() == propertyInfo)
                {
                    builder_front.Remove(builder_front.Length - 1, 1);
                    builder_front.Append(")");

                    builder_behind.Remove(builder_behind.Length - 1, 1);
                    builder_behind.Append(")");
                }
            }
            //Generate SqlStatement
            return DbContext.SqlStatement = builder_front.Append(builder_behind.ToString()).ToString().TrimEnd();
        }

        public override string Delete<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            DbContext.Parameters = new Dictionary<string, object>();
            DbContext.TableName = TableAttribute.GetName(typeof(TEntity));
            DbContext.SqlStatement = $"DELETE FROM {DbContext.TableName} {filter.Parameters[0].Name} {LambdaToSql.ConvertWhere(filter, DbContext.Parameters)}".TrimEnd();
            return DbContext.SqlStatement;
        }
        public override string Delete<TEntity>(TEntity entity)
        {
            DbContext.Parameters = new Dictionary<string, object>();
            DbContext.TableName = TableAttribute.GetName(typeof(TEntity));
            PropertyInfo[] propertyInfos = GetPropertiesDicByType(typeof(TEntity));
            //get property which is key
            var property = propertyInfos.Where(t => t.GetCustomAttribute(typeof(PrimaryKeyBaseAttribute), true) is PrimaryKeyBaseAttribute)?.FirstOrDefault();
            if (property == null)
                throw new TableKeyNotFoundException($"table '{DbContext.TableName}' not found key column");

            string colunmName = property.Name;
            var value = property.GetValue(entity);

            if (property.GetCustomAttribute(typeof(ColumnBaseAttribute), true) is ColumnBaseAttribute columnAttr)
                colunmName = columnAttr.GetName(property.Name);

            DbContext.Parameters.AddOrUpdate($"{ParametricFlag}t{colunmName}", value);
            return DbContext.SqlStatement = $"DELETE t FROM {DbContext.TableName} t WHERE t.{colunmName} = {ParametricFlag}t{colunmName}".TrimEnd();
        }
        public override string Update<TEntity>(Expression<Func<TEntity, bool>> filter, TEntity entity)
        {
            DbContext.Parameters = new Dictionary<string, object>();
            DbContext.TableName = TableAttribute.GetName(typeof(TEntity));

            StringBuilder builder_front = new StringBuilder();
            builder_front.Append("UPDATE ");
            builder_front.Append(DbContext.TableName);
            builder_front.Append(" ");

            //查询语句中表的别名，例如“t”
            string alias = filter.Parameters[0].Name;
            builder_front.Append(alias);
            builder_front.Append(" SET ");

            PropertyInfo[] propertyInfos = GetPropertiesDicByType(typeof(TEntity));
            string columnName = string.Empty;
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                //AutoIncrease : if property is auto increase attribute skip this column.
                if (propertyInfo.GetCustomAttribute(typeof(AutoIncreaseBaseAttribute), true) is AutoIncreaseBaseAttribute autoIncreaseAttr)
                {
                }
                //Column :
                else if (propertyInfo.GetCustomAttribute(typeof(ColumnBaseAttribute), true) is ColumnBaseAttribute columnAttr && base._columns.Contains(columnAttr.GetName(propertyInfo.Name)))
                {
                    builder_front.Append(columnAttr.GetName(propertyInfo.Name));
                    builder_front.Append("=");
                    builder_front.Append($"{ParametricFlag}{alias}");
                    columnName = columnAttr.GetName(propertyInfo.Name).Replace("`", "");
                    builder_front.Append(columnName);
                    builder_front.Append(",");

                    DbContext.Parameters.AddOrUpdate($"{ParametricFlag}{alias}{columnName}", propertyInfo.GetValue(entity));
                }
                //in the end,remove the redundant symbol of ','
                if (propertyInfos.Last() == propertyInfo)
                {
                    builder_front.Remove(builder_front.Length - 1, 1);
                }
            }

            //Generate SqlStatement
            return DbContext.SqlStatement = builder_front.Append($"{LambdaToSql.ConvertWhere(filter, DbContext.Parameters)}").ToString().TrimEnd();
        }
        public override string Update<TEntity>(TEntity entity, out Expression<Func<TEntity, bool>> filter)
        {
            DbContext.Parameters = new Dictionary<string, object>();
            DbContext.TableName = TableAttribute.GetName(typeof(TEntity));
            PropertyInfo[] propertyInfos = GetPropertiesDicByType(typeof(TEntity));

            filter = CreateUpdateWhereFilter<TEntity>(entity);

            //开始构造赋值的sql语句
            StringBuilder builder_front = new StringBuilder();
            builder_front.Append("UPDATE ");
            builder_front.Append(DbContext.TableName);
            builder_front.Append(" ");

            //查询语句中表的别名，例如“t”
            string alias = filter.Parameters[0].Name;

            builder_front.Append(alias);
            builder_front.Append(" SET ");

            string columnName = string.Empty;
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                //AutoIncrease : if property is auto increase attribute skip this column.
                if (propertyInfo.GetCustomAttribute(typeof(AutoIncreaseBaseAttribute), true) is AutoIncreaseBaseAttribute autoIncreaseAttr)
                {
                }
                //Column :
                else if (propertyInfo.GetCustomAttribute(typeof(ColumnBaseAttribute), true) is ColumnBaseAttribute columnAttr)
                {
                    builder_front.Append(columnAttr.GetName(propertyInfo.Name));
                    builder_front.Append("=");
                    builder_front.Append($"{ParametricFlag}{alias}");
                    columnName = columnAttr.GetName(propertyInfo.Name).Replace("`", "");
                    builder_front.Append(columnName);
                    builder_front.Append(",");

                    DbContext.Parameters.AddOrUpdate($"{ParametricFlag}{alias}{columnName}", propertyInfo.GetValue(entity));
                }
                //in the end,remove the redundant symbol of ','
                if (propertyInfos.Last() == propertyInfo)
                {
                    builder_front.Remove(builder_front.Length - 1, 1);
                }
            }

            //Generate SqlStatement
            return DbContext.SqlStatement = builder_front.Append($"{LambdaToSql.ConvertWhere(filter, DbContext.Parameters)}").ToString().TrimEnd();
        }
        public override string QueryableCount<TEntity>()
        {
            return DbContext.SqlStatement = $"SELECT COUNT(0) FROM {DbContext.TableName} {_alias} {_where}".TrimEnd();
        }
        public override string QueryableQuery<TEntity>()
        {
            string queryColumns = (_columns == null || !_columns.Any()) ? "*" : string.Join(",", _columns.Select(t => $"{_alias}.{t}").ToArray());
            return DbContext.SqlStatement = $"SELECT {queryColumns} FROM {DbContext.TableName} {_alias} {_where} {_limit} {_orderBy} ".TrimEnd();
        }

        public override string QueryablePaging<TEntity>()
        {
            string queryColumns = (_columns == null || !_columns.Any()) ? "*" : string.Join(",", _columns.Select(t => $"{_alias}.{t}").ToArray());
            string originalSQL = $"SELECT {queryColumns} FROM {DbContext.TableName} {_alias} {_where} {_orderBy}";
            return DbContext.SqlStatement = FormatPagingSQL(originalSQL);

        }

        public override string QueryableAny<TEntity>()
        {
            SetLimit(1);
            return DbContext.SqlStatement = $"SELECT 1 FROM {DbContext.TableName} {_alias} {_where} {_limit}".TrimEnd();
        }
    }
}
