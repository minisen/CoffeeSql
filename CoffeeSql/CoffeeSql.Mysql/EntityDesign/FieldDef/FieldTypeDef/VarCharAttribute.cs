using CoffeeSql.Core.CodeFirst.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CoffeeSql.Mysql.EntityDesign.FieldDef.FieldTypeDef
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class VarCharAttribute : FieldTypeDefBase, ICodeFirstFieldDef
    {
        public VarCharAttribute(int length) : base(length) { }

        public string getFieldDefDDLItem(PropertyInfo fieldPropertyInfo = null)
        {
            return $"varchar({this.Length})";
        }
    }
}
