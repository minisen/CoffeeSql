using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.Exceptions
{
    public class TableKeyNotFoundException : Exception
    {
        public TableKeyNotFoundException(string message) : base(message)
        {
        }
    }
}
