using System;
using System.Collections.Generic;
using System.Text;
using CoffeeSql.Core.CacheManagement.Interface;
using CoffeeSql.Core.Helpers;

namespace CoffeeSql.Core.CacheManagement.CacheOperater
{
    /// <summary>
    /// Local Cache Operater
    /// </summary>
    internal class LocalCacheOperater : IDBCacheOperate
    {
        public void Delete(string key)
        {
            MemoryCacheHelper.Delete(key);
        }

        public T Get<T>(string key)
        {
            return MemoryCacheHelper.Get<string, T>(key);
        }

        public bool IsExist<TValue>(string key, out TValue value)
        {
            return MemoryCacheHelper.Exist<string, TValue>(key, out value);
        }

        public void Put<T>(string key, T value, TimeSpan expiredTime)
        {
            MemoryCacheHelper.Put<string, T>(key, value, expiredTime);
        }
    }
}
