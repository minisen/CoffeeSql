using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeSql.Core.EntityDesign.Attributes
{
    /// <summary>
    /// Mark this Attribute for DbContext which be mapped to database
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DataBaseAttribute : Attribute
    {
        public DataBaseAttribute() { }
        public DataBaseAttribute(string databaseName)
        {
            this.Name = databaseName;
        }

        public string Name { get; private set; }

        public static string GetName(Type type)
        {
            var attr = type.GetCustomAttributes(typeof(DataBaseAttribute), true)?.FirstOrDefault();
            return (attr as DataBaseAttribute)?.Name ?? type.Name;
        }
    }
}
