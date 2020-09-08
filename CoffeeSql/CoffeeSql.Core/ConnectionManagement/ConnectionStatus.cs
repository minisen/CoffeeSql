using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.ConnectionManagement
{
    /// <summary>
    /// the Description of DB Connection status
    /// </summary>
    internal class ConnectionStatus
    {
        public int HashKey { get; set; }
        public string ConnectionString { get; set; }
        public int Count { get; set; }
    }
}
