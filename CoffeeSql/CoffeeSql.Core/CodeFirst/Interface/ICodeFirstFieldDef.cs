using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CoffeeSql.Core.CodeFirst.Interface
{
    /// <summary>
    /// Table field definition Attribute need to implement this interface to cooperate with CodeFirst
    /// </summary>
    internal interface ICodeFirstFieldDef
    {
        string getFieldDefDDLItem(PropertyInfo fieldPropertyInfo = null);
    }
}
