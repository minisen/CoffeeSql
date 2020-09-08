using CoffeeSql.Core.CodeFirst.Interface;
using CoffeeSql.Core.EntityDesign.Attributes;
using CoffeeSql.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CoffeeSql.Mysql.EntityDesign.FieldDef
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ColumnAttribute : ColumnBaseAttribute, ICodeFirstFieldDef
    {
        public ColumnAttribute() : base() 
        {
            this.SortNum = (int)FieldDefSORT.CLOUMN;
        }
        public ColumnAttribute(string columnName) : base(columnName) 
        {
            this.SortNum = (int)FieldDefSORT.CLOUMN;
        }

        public string getFieldDefDDLItem(PropertyInfo fieldPropertyInfo = null)
        {
            string fieldNameFieldDef;

            if (fieldPropertyInfo.Name is string defaultFieldName)
            {
                fieldNameFieldDef = $"{this.GetName(defaultFieldName)}";
            }
            else if (null == fieldPropertyInfo)
            {
                string fieldName = this.GetName(string.Empty);

                if (string.IsNullOrEmpty(fieldName))
                {
                    throw new EntityBaseFieldNotExistException("fieldName by property is null or empty");
                }
                else
                {
                    fieldNameFieldDef = $"{fieldName}";
                }
            }
            else
            {
                throw new ArgumentException("type of param must be string or param be null");
            }

            return fieldNameFieldDef;
        }
    }
}
