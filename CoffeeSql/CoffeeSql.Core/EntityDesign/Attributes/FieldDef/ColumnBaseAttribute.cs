using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeSql.Core.EntityDesign.Attributes
{
    /// <summary>
    /// Mark this Attribute for Entity Class's Property which be mapped to data table's cloumn
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ColumnBaseAttribute : FieldDefBaseAttribute
    {
        public ColumnBaseAttribute() { }
        public ColumnBaseAttribute(string columnName)
        {
            this.Name = columnName;
        }

        public string Name { get; private set; }

        public string GetName(string @default) => this.Name ?? @default;

        public static string GetName(Type type)
        {
            var attr = type.GetCustomAttributes(typeof(ColumnBaseAttribute), true)?.FirstOrDefault();
            return (attr as ColumnBaseAttribute)?.Name ?? type.Name;
        }
        public static string GetName(Type type, string defaultName)
        {
            var attr = type.GetCustomAttributes(typeof(ColumnBaseAttribute), true)?.FirstOrDefault();
            return (attr as ColumnBaseAttribute)?.Name ?? defaultName;
        }
    }
}
