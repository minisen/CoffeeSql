using System;

namespace CoffeeSql.Core.ExternalInterface
{
    /// <summary>
    /// Universal Interface of DbContext
    /// </summary>
    public interface IDbContext:IDisposable, ICacheable
    {
    }
}
