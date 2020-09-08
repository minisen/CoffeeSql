using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.Helpers
{
    internal static class MemoryCacheHelper
    {
        private static IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public static TValue Put<TKey, TValue>(TKey key, TValue value) => _cache.Set(key, value);
        public static TValue Put<TKey, TValue>(TKey key, TValue value, TimeSpan absoluteExpirationRelativeToNow) => _cache.Set(key, value, absoluteExpirationRelativeToNow);
        public static TValue Put<TKey, TValue>(TKey key, TValue value, DateTime absoluteExpiration) => _cache.Set(key, value, absoluteExpiration - DateTime.Now);
        public static TValue Get<TKey, TValue>(TKey key)
        {
            if (Exist(key))
            {
                return _cache.Get<TValue>(key);
            }
            return default(TValue);
        }

        public static bool Exist<TKey>(TKey key) => _cache.TryGetValue(key, out object value);
        public static bool Exist<TKey, TValue>(TKey key, out TValue value) => _cache.TryGetValue(key, out value);
        public static void Delete<TKey>(TKey key) => _cache.Remove(key);
    }
}
