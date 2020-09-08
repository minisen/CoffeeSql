using CoffeeSql.Core.Configs;
using System;
using System.Collections.Generic;
using System.Text;
using CoffeeSql.Core.Extensions;
using System.Linq;

namespace CoffeeSql.Core.ConnectionManagement
{
    /// <summary>
    /// the Manager of DB Connection
    /// </summary>
    public class ConnectionManager
    {
        /// <summary>
        /// constructed function
        /// </summary>
        /// <param name="connectionString_Write">the master Database ConnectionString (Only one)</param>
        /// <param name="connectionStrings_Read">the slave Database ConnectionStrings</param>
        internal ConnectionManager(string connectionString_Write, string[] connectionStrings_Read)
        {
            this.ConnectionString_Write = connectionString_Write.ToFormativeConnectionString();
            this.ConnectionStrings_Read = connectionStrings_Read?.Select(c => c.ToFormativeConnectionString()).Distinct()?.ToArray();

            this.CurrentConnectionString = this.ConnectionString_Write;

            //don't need Load Balance Strategy
            if (ConnectionStrings_Read == null || !ConnectionStrings_Read.Any() || ConnectionStrings_Read.Length == 1)
                return;

            this.connectionStatuses = new List<ConnectionStatus>();

            //Initialize connection usage collection
            ConnectionStrings_Read.Distinct().ToArray().Foreach(item => connectionStatuses.Add(new ConnectionStatus { HashKey = item.GetHashCode(), ConnectionString = item, Count = 0 }));

        }
        /// <summary>
        /// the master Database ConnectionString (Only one)
        /// </summary>
        public string ConnectionString_Write { get; private set; }
        /// <summary>
        /// the slave Database ConnectionStrings
        /// </summary>
        public string[] ConnectionStrings_Read { get; private set; }
        /// <summary>
        /// Database LoadBalanceStrategy Type
        /// </summary>
        public LoadBalanceStrategy ConnectionLoadBalanceStrategy { get; protected set; } = LoadBalanceStrategy.LeastConnection;
        /// <summary>
        /// the ConnectionString be used currently
        /// </summary>
        public string CurrentConnectionString { get; private set; }
        /// <summary>
        /// the next ConnectionString
        /// </summary>
        public string NextConnectionString { get; private set; }

        /// <summary>
        /// connection string usage
        /// </summary>
        IList<ConnectionStatus> connectionStatuses = null;
        /// <summary>
        /// Set the connection string to be used in the next execution,the effect will be maintained once
        /// </summary>
        /// <param name="connectionNext"></param>
        public void SetNextConnectionString(string connectionNext) => NextConnectionString = connectionNext.ToFormativeConnectionString();

        /// <summary>
        /// set ConnectionString currently be used
        /// </summary>
        /// <param name="operationType"></param>
        internal void SetConnectionString(OperationType operationType)
        {
            //first,Verify the next connection string 
            if (!string.IsNullOrEmpty(NextConnectionString))
            {
                CurrentConnectionString = NextConnectionString;
                NextConnectionString = string.Empty;
                return;
            }

            //write operation
            if (operationType == OperationType.Write)
            {
                CurrentConnectionString = ConnectionString_Write;
                return;
            }

            //read operation
            if (ConnectionStrings_Read == null || !ConnectionStrings_Read.Any())
            {
                CurrentConnectionString = ConnectionString_Write;
            }
            else if (ConnectionStrings_Read.Length == 1)
            {
                CurrentConnectionString = ConnectionStrings_Read[0];
            }
            else
            {
                if (connectionStatuses == null)
                    throw new NullReferenceException("Connection status list is null,please call Init() first!");

                switch (ConnectionLoadBalanceStrategy)
                {
                    case LoadBalanceStrategy.RoundRobin:
                        CurrentConnectionString = GetByRoundRobin();
                        break;
                    case LoadBalanceStrategy.LeastConnection:
                        CurrentConnectionString = LeastConnection();
                        break;
                    default:
                        CurrentConnectionString = LeastConnection();
                        break;
                }
            }
        }

        /// <summary>
        /// Get connection string by Round Robin
        /// </summary>
        /// <returns></returns>
        private string GetByRoundRobin()
        {
            var current = connectionStatuses.FirstOrDefault(t => t.HashKey == CurrentConnectionString.GetHashCode());
            if (current == null)
                throw new KeyNotFoundException("current connection not fount in connection strings,please check the connection list has been change");
            
            int currentIndex = connectionStatuses.IndexOf(current);

            if (currentIndex < connectionStatuses.Count)
                return connectionStatuses.ElementAt(currentIndex + 1).ConnectionString;
            else
                return connectionStatuses.First().ConnectionString;
        }

        /// <summary>
        /// Get connection string which Least used
        /// </summary>
        /// <returns></returns>
        private string LeastConnection()
        {
            var current = connectionStatuses.OrderBy(t => t.Count).First();
            current.Count++;
            return current.ConnectionString;
        }
    }
}
