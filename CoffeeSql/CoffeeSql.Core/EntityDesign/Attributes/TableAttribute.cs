using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CoffeeSql.Core.EntityDesign.Attributes
{
    /// <summary>
    /// Mark this Attribute for Entity Class which be mapped to data table
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TableAttribute : Attribute
    {
        public TableAttribute() { }
        public TableAttribute(string tableName)
        {
            this.Name = tableName;
        }

        public string Name { get; private set; }

        public static string GetName(Type type)
        {
            var attr = type.GetCustomAttributes(typeof(TableAttribute), true)?.FirstOrDefault();
            return (attr as TableAttribute)?.Name ?? type.Name;
        }
    }
}
