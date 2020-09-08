using CoffeeSql.Core.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CoffeeSql.Core.CacheManagement
{
    /// <summary>
    /// Database Cache Manager
    /// The Cache of CoffeeSQL be divided into two levels, every level have itsself's switch:
    /// 1、the first level Cache(Query Cache) => Cache data in short sql queries
    /// 2、the second level Cache(Table Cache) => Cache entire table
    /// </summary>
    public class DbCacheManager : CacheManagerBase
    {
        internal DbCacheManager(DbContext context) : base(context)
        {
            QueryCacheManager = new QueryCacheManager(context);
            TableCacheManager = new TableCacheManager(context);
        }

        public QueryCacheManager QueryCacheManager { get; private set; }
        public TableCacheManager TableCacheManager { get; private set; }

        /// <summary>
        /// Clear All Cache
        /// </summary>
        public void FlushAllCache()
        {
            if (DbContext.OpenQueryCache)
                QueryCacheManager.FlushAllCache();
            if (DbContext.OpenTableCache)
                TableCacheManager.FlushAllCache();
        }
        /// <summary>
        /// Clear single table's all caching data
        /// </summary>
        public void FlushCurrentCollectionCache(string collectionName = null)
        {
            if (DbContext.OpenQueryCache)
                QueryCacheManager.FlushCollectionCache(collectionName);
            if (DbContext.OpenTableCache)
                TableCacheManager.FlushCollectionCache(collectionName);
        }

        //Same Logic:
        //1.Clear all cached records about the table in the query cache
        //2.Update the table record in the table cache
        internal void Add<TEntity>(TEntity entity)
        {
            if (DbContext.OpenQueryCache)
                QueryCacheManager.FlushCollectionCache();
            if (DbContext.OpenTableCache)
                TableCacheManager.AddCache(entity);
        }
        internal void Add<TEntity>(IEnumerable<TEntity> entities)
        {
            if (DbContext.OpenQueryCache)
                QueryCacheManager.FlushCollectionCache();
            if (DbContext.OpenTableCache)
                TableCacheManager.AddCache(entities);
        }
        internal void Update<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> filter, IEnumerable<string> updateCloumns = null)
        {
            if (DbContext.OpenQueryCache)
                QueryCacheManager.FlushCollectionCache();
            if (DbContext.OpenTableCache)
                TableCacheManager.UpdateCache(entity, filter, updateCloumns);
        }
        internal void Delete<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            if (DbContext.OpenQueryCache)
                QueryCacheManager.FlushCollectionCache();
            if (DbContext.OpenTableCache)
                TableCacheManager.DeleteCache(filter);
        }
        internal void Delete<TEntity>(TEntity entity)
        {
            if (DbContext.OpenQueryCache)
                QueryCacheManager.FlushCollectionCache();
            if (DbContext.OpenTableCache)
                TableCacheManager.DeleteCache(entity);
        }

        internal List<TEntity> GetEntities<TEntity>(Expression<Func<TEntity, bool>> filter, Func<List<TEntity>> func) where TEntity : class
        {
            List<TEntity> entities = null;

            //1.Determine whether query data exists in Table Cache, if not, begin to initialize Table Caching Operation
            if (DbContext.OpenTableCache)
                entities = TableCacheManager.GetEntitiesFromCache(filter);
            
            //2.Determine whether query data exists in Query Cache
            if (DbContext.OpenQueryCache)
                if (entities == null || !entities.Any())
                    entities = QueryCacheManager.GetEntitiesFromCache<List<TEntity>>();
            
            //3.if not in both Table Cache and Query Cache, query data from Query logic 
            if (entities == null || !entities.Any())
            {
                entities = func();
                DbContext.IsFromCache = false;
                //4.Cache Query result to Query Cache
                QueryCacheManager.CacheData(entities);
            }

            return entities;
        }
        internal TEntity GetEntity<TEntity>(Expression<Func<TEntity, bool>> filter, Func<TEntity> func) where TEntity : class
        {
            TEntity result = null;

            //1.Determine whether query data exists in Table Cache, if not, begin to initialize Table Caching Operation
            if (DbContext.OpenTableCache)
                result = TableCacheManager.GetEntitiesFromCache(filter)?.FirstOrDefault();

            //2.Determine whether query data exists in Query Cache
            if (DbContext.OpenQueryCache)
                if (result == null)
                    result = QueryCacheManager.GetEntitiesFromCache<TEntity>();

            //3.if not in both Table Cache and Query Cache, query data from Query logic 
            if (result == null || result == default(TEntity))
            {
                result = func();
                DbContext.IsFromCache = false;
                //4.Cache Query result to Query Cache
                QueryCacheManager.CacheData(result);
            }

            return result;
        }
        internal long GetCount<TEntity>(Expression<Func<TEntity, bool>> filter, Func<long> func) where TEntity : class
        {
            long? result = null;

            //1.Determine whether query data exists in Table Cache, if not, begin to initialize Table Caching Operation
            if (DbContext.OpenTableCache)
                result = TableCacheManager.GetEntitiesFromCache(filter)?.Count;

            //2.Determine whether query data exists in Query Cache
            if (DbContext.OpenQueryCache)
                if (result == null)
                    result = QueryCacheManager.GetEntitiesFromCache<long?>();

            //3.if not in both Table Cache and Query Cache, query data from Query logic 
            if (result == null || result == default(long))
            {
                result = func();
                DbContext.IsFromCache = false;
                //4.Cache Query result to Query Cache
                QueryCacheManager.CacheData(result);
            }

            return result ?? default(long);
        }
        internal T GetObject<T>(Func<T> func) where T : class
        {
            T result = null;

            //1.Determine whether query data exists in Query Cache
            if (DbContext.OpenTableCache)
                result = QueryCacheManager.GetEntitiesFromCache<T>();

            //2.if not in Query Cache, query data from Query logic 
            if (result == null)
            {
                result = func();
                DbContext.IsFromCache = false;
                //3.Cache Query result to Query Cache
                QueryCacheManager.CacheData(result);
            }

            return result;
        }
    }
}
