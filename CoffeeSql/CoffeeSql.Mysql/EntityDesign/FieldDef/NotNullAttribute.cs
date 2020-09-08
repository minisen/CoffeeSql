using CoffeeSql.Core.CodeFirst.Interface;
using CoffeeSql.Core.EntityDesign.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CoffeeSql.Mysql.EntityDesign.FieldDef
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class NotNullAttribute : FieldDefBaseAttribute, ICodeFirstFieldDef
    {
        public NotNullAttribute()
        {
            this.SortNum = (int)FieldDefSORT.NOTNULL;
        }

        public string getFieldDefDDLItem(PropertyInfo fieldPropertyInfo = null)
        {
            return "NOT NULL";
        }
    }
}
