using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.Exceptions
{
    public class EntityBaseFieldNotExistException : Exception
    {
        public EntityBaseFieldNotExistException(string errorMsg):base(errorMsg)
        {
        }
    }
}
