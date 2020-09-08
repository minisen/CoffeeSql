using CoffeeSql.Core.CodeFirst.Interface;
using CoffeeSql.Core.EntityDesign.Attributes;
using CoffeeSql.Core.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CoffeeSql.Core.CodeFirst
{
    /// <summary>
    /// the base class of DDLTextGenerator 
    /// </summary>
    public abstract class DDLTextGeneratorBase
    {
        #region for example

        /*
         
         DROP TABLE IF EXISTS `b_example`;                                  =>  create by method DropTableIfExist(string tableName)
         CREATE TABLE `b_example`                                           =>  create by CREATE_TABLE
         (
             `Id` varchar(50) NOT NULL PRIMARY KEY,
             `RemarkInfo` varchar(100) DEFAULT '0',
             `DelFlag` tinyint(1) unsigned zerofill NOT NULL DEFAULT '0'    =>  create by method FieldDefDDL(Type tEntityType)
         )
         ENGINE=InnoDB DEFAULT CHARSET=utf8;                                =>  create by CREATE_TABLE_AtLast

         */

        #endregion

        #region DDL syntax keywords

        public string CREATE_TABLE { get; protected set; } = "CREATE TABLE";

        public string CREATE_TABLE_AtLast { get; protected set; } = string.Empty;

        #endregion

        #region Cache properties by type
        private static ConcurrentDictionary<Type, PropertyInfo[]> _propertiesDic = new ConcurrentDictionary<Type, PropertyInfo[]>();
        protected static PropertyInfo[] GetPropertiesDicByType(Type type)
        {
            if (_propertiesDic.ContainsKey(type))
            {
                return _propertiesDic[type];
            }
            else
            {
                PropertyInfo[] res = type.GetProperties();
                _propertiesDic.AddOrUpdate(type, res);

                return res;
            }
        }
        #endregion

        private string FieldDefDDL(PropertyInfo tFieldPropertyInfo)
        {
            var fieldDefAttrs = tFieldPropertyInfo.GetCustomAttributes(typeof(FieldDefBaseAttribute), true)
                                                  .Select(attr => attr as FieldDefBaseAttribute)
                                                  .OrderBy(attr => attr.SortNum);

            string[] fieldDefItems = fieldDefAttrs.Where(attr => attr is ICodeFirstFieldDef)
                                                  .Select(attr => attr as ICodeFirstFieldDef)
                                                  .Select(defItem => defItem.getFieldDefDDLItem(tFieldPropertyInfo))
                                                  .ToArray();

            return string.Join(" ", fieldDefItems);
        }
        protected abstract string TableName(Type tEntityType);
        protected abstract string DropTableIfExist(string tableName);

        public string CreateTable(Type tEntityType)
        {
            string tableName = TableName(tEntityType);

            StringBuilder createtableSB = new StringBuilder("\r\n");

            //DropTableIfExist
            createtableSB.Append(DropTableIfExist(tableName)).Append(";\r\n");

            //Create Table
            createtableSB.Append(this.CREATE_TABLE).Append($" {tableName}\r\n");

            //Field Definations
            createtableSB.Append("(\r\n");
            var propertyInfos = GetPropertiesDicByType(tEntityType).Where(propertyInfo => propertyInfo.GetCustomAttribute(typeof(ColumnBaseAttribute), true) is ColumnBaseAttribute).ToList();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                createtableSB.Append($"    {this.FieldDefDDL(propertyInfo)}")
                             .Append((propertyInfos.IndexOf(propertyInfo) != propertyInfos.Count - 1) ? ",\r\n" : "\r\n");
            }
            createtableSB.Append(")");

            //At Last
            createtableSB.Append(string.IsNullOrEmpty(this.CREATE_TABLE_AtLast) ? ";" : $"\r\n{this.CREATE_TABLE_AtLast};");

            return createtableSB.ToString();
        }

        /*   oracle
declare
    v_count number;
begin
    select count(1) into v_count from user_tables t where t.TABLE_NAME='T_TEST';
if v_count>0 then
    execute immediate'drop table T_TEST';
end if;
end;
         */
    }
}
