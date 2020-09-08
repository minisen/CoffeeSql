using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.CacheManagement.Interface
{
    /// <summary>
    /// the Interface of Cache Operation
    /// </summary>
    internal interface IDBCacheOperate
    {
        bool IsExist<TValue>(string key, out TValue value);
        void Put<T>(string key, T value, TimeSpan expiredTime);
        T Get<T>(string key);
        void Delete(string key);
    }
}
