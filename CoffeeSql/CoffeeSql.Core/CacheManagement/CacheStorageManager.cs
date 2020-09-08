using CoffeeSql.Core.CacheManagement.CacheOperater;
using CoffeeSql.Core.Configs;
using CoffeeSql.Core.DbContexts;
using CoffeeSql.Core.CacheManagement.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.CacheManagement
{
    /// <summary>
    /// Cache Storage Manager
    /// </summary>
    internal class CacheStorageManager
    {
        internal DbContext DbContext;
        private IDBCacheOperate DBCacheOperater;

        internal CacheStorageManager(DbContext context)
        {
            this.DbContext = context;

            //switch a cache type
            switch (this.DbContext.CacheMediaType)
            {
                case CacheMediaType.Local:
                    this.DBCacheOperater = new LocalCacheOperater();
                    break;
                default:
                    this.DBCacheOperater = new LocalCacheOperater();
                    break;
            }
        }

        internal bool IsExist(string key)
        {
            object obj;
            return IsExist(key, out obj);
        }

        internal void Delete(string key)
        {
            DBCacheOperater.Delete(key);
        }

        internal T Get<T>(string key)
        {
            return DBCacheOperater.Get<T>(key);
        }

        internal bool IsExist<TValue>(string key, out TValue value)
        {
            return DBCacheOperater.IsExist<TValue>(key, out value);
        }

        internal void Put<T>(string key, T value, TimeSpan expiredTime)
        {
            DBCacheOperater.Put<T>(key, value, expiredTime);
        }
    }
}
