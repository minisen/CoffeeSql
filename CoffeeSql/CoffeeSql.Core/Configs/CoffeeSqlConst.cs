﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.Configs
{
    /// <summary>
    /// CoffeeSql Const Data
    /// </summary>
    internal static class CoffeeSqlConst
    {
        /// <summary>
        /// Sql Query Timeout
        /// </summary>
        internal static readonly int CommandTimeout = 60;
        /// <summary>
        /// 每张表一级缓存的最大个数，超出数目将会按从早到晚的顺序移除缓存键
        /// </summary>
        internal static readonly int QueryCacheMaxCountPerTable = 50;
        /// <summary>
        /// QueryCache默认过期时间
        /// </summary>
        internal static readonly TimeSpan QueryCacheExpiredTimeSpan = TimeSpan.FromMinutes(10);
        /// <summary>
        /// TableCache默认过期时间
        /// </summary>
        internal static readonly TimeSpan TableCacheExpiredTimeSpan = TimeSpan.FromHours(6);
        /// <summary>
        /// 默认缓存存储媒介
        /// </summary>
        internal static readonly CacheMediaType CacheMediaType = CacheMediaType.Local;
        /// <summary>
        /// QueryCache前缀
        /// </summary>
        internal const string CacheKey_QueryCache = "CoffeeSqlQueryCache__";
        /// <summary>
        /// TableCache前缀
        /// </summary>
        internal const string CacheKey_TableCache = "CoffeeSqlTableCache_";
        /// <summary>
        /// 表扫描key前缀
        /// </summary>
        internal const string CacheKey_TableScanning = "CoffeeSqlCacheScaning_";
        /// <summary>
        /// TableCache扫描键最多存在时间
        /// </summary>
        internal static readonly TimeSpan SpanScaningKeyExpiredTime = TimeSpan.FromMinutes(20);
        /// <summary>
        /// Table缓存键缓存的key前缀(用于保存所有的TableCache keys)
        /// </summary>
        internal const string CacheKey_TableCacheKeys = "CoffeeSqlTableCacheKeys__";
        /// <summary>
        /// Query缓存键缓存的key前缀(用于保存所有的QueryCache keys)
        /// </summary>
        internal const string CacheKey_QueryCacheKeys = "CoffeeSqlQueryCacheKeys__";
        /// <summary>
        /// 缓存键缓存的最大时间，该值只是个默认时间，保证在该配置中最大集合，实际动态计算为最大时间
        /// </summary>
        internal static readonly TimeSpan CacheKeysMaxExpiredTime = TimeSpan.FromDays(1);

        internal static string GetQueryCacheKeysCacheKey(string dataBaseName) => $"{CacheKey_QueryCacheKeys}{dataBaseName}";
        internal static string GetTableCacheKeysCacheKey(string dataBaseName) => $"{CacheKey_TableCacheKeys}{dataBaseName}";
    }
}
