﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.Extensions
{
    internal static class GenericExtension
    {
        /// <summary>
        /// If exist the key , update the value.
        /// </summary>
        /// <typeparam name="TKey">type of Key</typeparam>
        /// <typeparam name="TValue">type of Value</typeparam>
        /// <param name="dictionary">dictionary</param>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }
        /// <summary>
        /// Safe get,if key exist,return value. if not return default(TValue).
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue SafeGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary.TryGetValue(key, out TValue value))
                return value;
            return default(TValue);
        }
        /// <summary>
        /// Foreach
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource"></param>
        /// <param name="func"></param>
        public static void Foreach<T>(this IEnumerable<T> dataSource, Action<T> func)
        {
            foreach (var dataItem in dataSource)
            {
                func(dataItem);
            }
        }
    }
}
