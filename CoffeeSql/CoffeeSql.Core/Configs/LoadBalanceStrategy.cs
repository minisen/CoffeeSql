using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.Configs
{
    /// <summary>
    /// DataBase LoadBalance Strategy
    /// </summary>
    public enum LoadBalanceStrategy
    {
        /// <summary>
        /// take out in order
        /// </summary>
        RoundRobin,
        /// <summary>
        /// take out connection which least be used
        /// </summary>
        LeastConnection
    }
}
