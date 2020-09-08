using System;
using CoffeeSql.Core.CodeFirst.Interface;
using CoffeeSql.Core.EntityDesign.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CoffeeSql.Mysql.EntityDesign.FieldDef
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DefaultAttribute : AutoIncreaseBaseAttribute, ICodeFirstFieldDef
    {
        public object defaultValue = null;

        public DefaultAttribute()
        {
            this.SortNum = (int)FieldDefSORT.DEFAULT;
        }

        public DefaultAttribute(object value)
        {
            this.SortNum = (int)FieldDefSORT.DEFAULT;
            this.defaultValue = value;
        }

        public string getFieldDefDDLItem(PropertyInfo fieldPropertyInfo = null)
        {
            string res = string.Empty;

            if (null == defaultValue)
            {
                res = "DEFAULT NULL";
            }
            else if (this.defaultValue is int valueNum)
            {
                res = $"DEFAULT {valueNum}";
            }
            else if (this.defaultValue is string valueStr)
            {
                res = $"DEFAULT '{valueStr}'";
            }
            else if (this.defaultValue is Default value)
            {
                if (value == Default.CURRENT_TIMESTAMP)
                {
                    res = $"DEFAULT CURRENT_TIMESTAMP";
                }
            }
            else
            {
                throw new ArgumentException("the type of default value is not current, should be int,string or null");
            }

            return res;
        }
    }

    public enum Default
    {
        CURRENT_TIMESTAMP = 0
    }
}
