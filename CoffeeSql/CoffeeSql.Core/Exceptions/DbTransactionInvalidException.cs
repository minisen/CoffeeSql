using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.Exceptions
{
    public class DbTransactionInvalidException : Exception
    {
        public DbTransactionInvalidException(string message) : base(message)
        {
        }
    }
}
