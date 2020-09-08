using CoffeeSql.Core.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.CacheManagement
{
    /// <summary>
    /// Cache Manager's base info
    /// </summary>
    public abstract class CacheManagerBase
    {
        protected DbContext DbContext;

        protected CacheManagerBase(DbContext context)
        {
            DbContext = context;
            CacheStorageManager = new CacheStorageManager(context);
        }

        internal CacheStorageManager CacheStorageManager { get; set; }
    }
}
