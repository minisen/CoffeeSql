using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.EntityDesign.Attributes
{
    /// <summary>
    /// Mark this Attribute for Entity Class's Property which be mapped to data table's primary key
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PrimaryKeyBaseAttribute : FieldDefBaseAttribute
    {
    }
}
