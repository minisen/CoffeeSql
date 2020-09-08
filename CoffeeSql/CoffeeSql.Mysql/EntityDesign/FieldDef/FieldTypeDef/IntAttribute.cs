using CoffeeSql.Core.CodeFirst.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CoffeeSql.Mysql.EntityDesign.FieldDef.FieldTypeDef
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IntAttribute : FieldTypeDefBase, ICodeFirstFieldDef
    {
        public IntAttribute()
        {
            this.Length = 11;
        }

        public IntAttribute(int length) : base(length) { }

        public string getFieldDefDDLItem(PropertyInfo fieldPropertyInfo = null)
        {
            return $"int({this.Length})";
        }
    }
}
