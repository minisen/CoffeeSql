using CoffeeSql.Core.Configs;
using CoffeeSql.Core.ConnectionManagement;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using CoffeeSql.Core.Extensions;
using CoffeeSql.Core.ExternalInterface;
using System.Threading.Tasks;
using System.Linq.Expressions;
using CoffeeSql.Core.CacheManagement;

namespace CoffeeSql.Core.DbContexts
{
    /// <summary>
    /// The Context of Database
    /// </summary>
    public abstract class DbContext : IDbContext, IBaseOperate
    {
        protected DbContext(string connectionString_Write, params string[] connectionStrings_Read)
        {
            if (string.IsNullOrEmpty(connectionString_Write))
            {
                throw new ArgumentNullException(nameof(connectionString_Write), "argument can not be null");
            }

            if (this.ConnectionManager == null)
            {
                this.ConnectionManager = new ConnectionManager(connectionString_Write, connectionStrings_Read);
            }

            DbCacheManager = new DbCacheManager(this);
        }

        #region Database Control

        /// <summary>
        /// Database Name
        /// </summary>
        public string DataBaseName { get; internal set; }
        /// <summary>
        /// equal to Database Table name
        /// </summary>
        public string CollectionName { get; internal set; }
        /// <summary>
        /// Database Type
        /// </summary>
        public DataBaseType DataBaseType { get; protected set; }

        #region Connection Control
        /// <summary>
        /// DB Connction string Manager
        /// </summary>
        public ConnectionManager ConnectionManager { get; }

        /// <summary>
        /// create DBConnection by connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        internal abstract DbConnection CreateDbConnection(string connectionString);

        #endregion

        /// <summary>
        /// Real execution persistent operation switch
        /// </summary>
        public bool OpenRealExecutionSaveToDb { get; protected set; } = true;
        #endregion

        #region Validate Control
        /// <summary>
        /// entity property data value Switch
        /// </summary>
        public bool OpenPropertyDataValidate { get; protected set; } = false;
        #endregion

        #region Cache Control 缓存管理

        /// <summary>
        /// 数据库缓存管理器
        /// </summary>
        public DbCacheManager DbCacheManager { get; internal set; }
        /// <summary>
        /// 一级缓存
        /// 查询条件级别的缓存（filter），可以暂时缓存根据查询条件查询到的数据
        /// 如果开启二级缓存，且当前操作对应的表已经在二级缓存里，则不进行条件缓存
        /// </summary>
        public bool OpenQueryCache { get; protected set; } = false;
        /// <summary>
        /// 二级缓存
        /// 配置表缓存标签对整张数据库表进行缓存
        /// </summary>
        public bool OpenTableCache { get; protected set; } = false;
        /// <summary>
        /// 查询缓存的默认缓存时间
        /// </summary>
        private TimeSpan _QueryCacheExpiredTimeSpan = CoffeeSqlConst.QueryCacheExpiredTimeSpan;
        public TimeSpan QueryCacheExpiredTimeSpan
        {
            get { return _QueryCacheExpiredTimeSpan; }
            protected set
            {
                if (value > MaxExpiredTimeSpan)
                {
                    MaxExpiredTimeSpan = value;
                }
                _QueryCacheExpiredTimeSpan = value;
            }
        }
        /// <summary>
        /// 表缓存的缓存时间
        /// </summary>
        private TimeSpan _TableCacheExpiredTimeSpan = CoffeeSqlConst.TableCacheExpiredTimeSpan;
        public TimeSpan TableCacheExpiredTimeSpan
        {
            get { return _TableCacheExpiredTimeSpan; }
            protected set
            {
                if (value > MaxExpiredTimeSpan)
                {
                    MaxExpiredTimeSpan = value;
                }
                _TableCacheExpiredTimeSpan = value;
            }
        }
        /// <summary>
        /// 每张表一级缓存的最大个数，超出数目将会按从早到晚的顺序移除缓存键
        /// </summary>
        public int QueryCacheMaxCountPerTable { get; protected set; } = CoffeeSqlConst.QueryCacheMaxCountPerTable;
        /// <summary>
        /// 数据是否从缓存中获取
        /// </summary>
        public bool IsFromCache { get; internal set; } = false;
        /// <summary>
        /// Cache 存储媒介,默认本地缓存
        /// </summary>
        public CacheMediaType CacheMediaType { get; protected set; } = CoffeeSqlConst.CacheMediaType;
        /// <summary>
        /// Cache 第三方存储媒介服务地址
        /// </summary>
        public string CacheMediaServer { get; protected set; }
        /// <summary>
        /// 最大的缓存时间（用于缓存缓存键）
        /// </summary>
        internal TimeSpan MaxExpiredTimeSpan { get; set; } = CoffeeSqlConst.CacheKeysMaxExpiredTime;
        /// <summary>
        /// 获取一级缓存的缓存键；如SQL中的sql语句和参数，作为一级缓存查询的key，这里根据不同的数据库自定义拼接
        /// </summary>
        /// <returns></returns>
        internal abstract string GetQueryCacheKey();
        /// <summary>
        /// 获取集合全部数据的内置方法，用于二级缓存
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        internal abstract List<TEntity> GetFullCollectionData<TEntity>() where TEntity : class;

        #endregion

        #region Operate Standard API
        public abstract int Add<TEntity>(TEntity entity) where TEntity : class;
        public abstract int Update<TEntity>(TEntity entity) where TEntity : class;
        public abstract int Delete<TEntity>(TEntity entity) where TEntity : class;
        public abstract int Delete<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;

        public abstract Task<int> AddAsync<TEntity>(TEntity entity) where TEntity : class;
        public abstract Task<int> UpdateAsync<TEntity>(TEntity entity) where TEntity : class;
        public abstract Task<int> DeleteAsync<TEntity>(TEntity entity) where TEntity : class;
        public abstract Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;

        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }
}
