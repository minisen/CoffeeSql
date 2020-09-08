using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CoffeeSql.Core.Validation
{
    /// <summary>
    /// Base Class of Validation Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class ValidationAttribute : Attribute
    {
        internal string ErrorMessage { get; set; }

        public ValidationAttribute(string errorMsg)
        {
            ErrorMessage = errorMsg;
        }

        // Method of Validating value, implemented by inheritance class
        public abstract void Verify(PropertyInfo propertyInfo, object value);
    }
}
