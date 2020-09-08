using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.EntityDesign.Attributes
{
    /// <summary>
    /// Mark this Attribute for Entity Class's Property which value can auto increase
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutoIncreaseBaseAttribute : FieldDefBaseAttribute
    {
    }
}
