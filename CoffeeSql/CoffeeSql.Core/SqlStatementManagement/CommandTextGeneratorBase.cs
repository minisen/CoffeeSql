using CoffeeSql.Core.EntityDesign.Attributes;
using CoffeeSql.Core.DbContexts;
using CoffeeSql.Core.Exceptions;
using CoffeeSql.Core.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CoffeeSql.Core.SqlStatementManagement
{
    /// <summary>
    /// Sql命令生成器基类
    /// </summary>
    internal abstract class CommandTextGeneratorBase
    {
        //add timestamp to table alias for getting table Alias which no repeat,length <= 20
        protected static string tableAlias = $"t_{(DateTime.Now.Ticks / 10000000000).ToString()}";

        public CommandTextGeneratorBase(SqlDbContext _dbContext)
        {
            this.DbContext = _dbContext;

            LambdaToSql.ParametricFlag = ParametricFlag;
        }

        protected SqlDbContext DbContext;

        /// <summary>
        /// Get Parametric Flag，like ORACLE is ":";MYSQL is "@"
        /// </summary>
        /// <returns></returns>
        protected abstract string ParametricFlag { get; }

        #region 查询语句关键元素

        public string _mainSqlStatement { get; protected set; }    //sql查詢主語句
        public object[] _params { get; protected set; }           //參數化查詢參數集合
        public int _pageIndex { get; protected set; }
        public int _pageSize { get; protected set; }
        public List<string> _columns { get; protected set; }
        public string _where { get; protected set; }
        public string _orderBy { get; protected set; }
        public string _alias { get; protected set; }
        public string _limit { get; protected set; }

        #endregion

        #region 设置关键字

        public abstract void SetSql_Params(string sqlStatement, object[] param);
        public abstract void SetWhere<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class;
        public abstract void SetOrderBy<TEntity>(Expression<Func<TEntity, object>> orderBy, bool isDesc) where TEntity : class;
        public abstract void SetPage(int pageIndex, int pageSize);
        public abstract void SetLimit(int count);
        public abstract void SetAlias(string alias);
        public abstract void SetColumns<TEntity>(Expression<Func<TEntity, object>> columns) where TEntity : class;

        #endregion

        #region Cache properties by type
        private static ConcurrentDictionary<Type, PropertyInfo[]> _propertiesDic = new ConcurrentDictionary<Type, PropertyInfo[]>();
        protected static PropertyInfo[] GetPropertiesDicByType(Type type)
        {
            if (_propertiesDic.ContainsKey(type))
            {
                return _propertiesDic[type];
            }
            else
            {
                PropertyInfo[] res = type.GetProperties();
                _propertiesDic.AddOrUpdate(type, res);

                return res;
            }
        }
        #endregion

        #region 增删改操作

        public abstract string Add<TEntity>(TEntity entity) where TEntity : class;
        public abstract string Update<TEntity>(Expression<Func<TEntity, bool>> filter, TEntity entity) where TEntity : class;
        public abstract string Update<TEntity>(TEntity entity, out Expression<Func<TEntity, bool>> filter) where TEntity : class;
        public abstract string Delete<TEntity>(TEntity entity) where TEntity : class;
        public abstract string Delete<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;

        /// <summary>
        /// create the 'where' filter for Update
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected Expression<Func<TEntity, bool>> CreateUpdateWhereFilter<TEntity>(TEntity entity)
        {
            //t=>t.'Key'== value

            Expression<Func<TEntity, bool>> filter = null;
            PropertyInfo[] propertyInfos = GetPropertiesDicByType(typeof(TEntity));

            //查找主键以及主键对应的值，如果用该方法更新数据，主键是必须存在的
            //get property which is key
            var keyProperty = propertyInfos.Where(t => t.GetCustomAttribute(typeof(PrimaryKeyBaseAttribute), true) is PrimaryKeyBaseAttribute)?.FirstOrDefault();
            if (keyProperty == null)
                throw new TableKeyNotFoundException($"table '{DbContext.TableName}' not found key column");

            //主键的key
            string keyName = keyProperty.Name;
            //主键的value 
            var keyValue = keyProperty.GetValue(entity);

            if (keyProperty.GetCustomAttribute(typeof(ColumnBaseAttribute), true) is ColumnBaseAttribute columnAttr1)
                keyName = columnAttr1.GetName(keyProperty.Name);

            ParameterExpression param = Expression.Parameter(typeof(TEntity), "t");
            MemberExpression left = Expression.Property(param, keyProperty);
            Expression where;

            ConstantExpression right = Expression.Constant(keyValue);
            where = Expression.Equal(left, right);

            filter = Expression.Lambda<Func<TEntity, bool>>(where, param);

            //将主键的查询参数加到字典中
            //DbContext.Parameters.AddOrUpdate($"{ParametricFlag}t{keyName}", keyValue);

            return filter;
        }

        #endregion

        #region Queryable Methods,返回的都是最终结果(弱類型)
        /// <summary>
        /// 生成查詢語句
        /// </summary>
        /// <returns></returns>
        public string QueryableQuery()
        {
            Tuple<string, IDictionary<string, object>> sqlCommandTuple = ProcessSqlCommand(_mainSqlStatement, _params);

            DbContext.Parameters = sqlCommandTuple.Item2;

            return DbContext.SqlStatement = sqlCommandTuple.Item1;
        }

        /// <summary>
        /// 生成分頁查詢語句
        /// </summary>
        /// <returns></returns>
        public string QueryablePaging()
        {
            Tuple<string, IDictionary<string, object>> sqlCommandTuple = ProcessSqlCommand(_mainSqlStatement, _params);

            DbContext.Parameters = sqlCommandTuple.Item2;

            return DbContext.SqlStatement = FormatPagingSQL(sqlCommandTuple.Item1);
        }

        /// <summary>
        /// 將sql語句轉化成參數化查詢格式
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private Tuple<string, IDictionary<string, object>> ProcessSqlCommand(string sqlCommand, params object[] param)
        {
            var tempParamKeyValDic = param.Select((item, i) => new KeyValuePair<string, object>($"{ParametricFlag}p" + i, item)).ToDictionary(k => k.Key, v => v.Value);

            var tempSqlCommand = string.Format(sqlCommand, tempParamKeyValDic.Keys.ToArray());

            return Tuple.Create(tempSqlCommand, (IDictionary<string, object>)tempParamKeyValDic);
        }

        #region 由於不同的數據庫類型，不同的細節邏輯由子類實現
        /// <summary>
        /// 格式化原始查詢sql語句為分頁查詢（由子類不同實現）
        /// </summary>
        /// <param name="originalSQL"></param>
        /// <returns></returns>
        protected abstract string FormatPagingSQL(string originalSQL);
        #endregion

        #endregion

        #region Queryable Methods,返回的都是最终结果(强类型)
        public abstract string QueryableCount<TEntity>() where TEntity : class;
        public abstract string QueryableAny<TEntity>() where TEntity : class;
        public abstract string QueryableQuery<TEntity>() where TEntity : class;
        public abstract string QueryablePaging<TEntity>() where TEntity : class;
        #endregion

    }
}
