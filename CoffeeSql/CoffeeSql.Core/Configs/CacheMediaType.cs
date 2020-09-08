using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.Configs
{
    /// <summary>
    /// Cache Storage Media
    /// </summary>
    public enum CacheMediaType
    {
        /// <summary>
        /// Local cache
        /// </summary>
        Local = 0,
        /// <summary>
        /// Redis cache
        /// </summary>
        Redis = 1
    }
}
