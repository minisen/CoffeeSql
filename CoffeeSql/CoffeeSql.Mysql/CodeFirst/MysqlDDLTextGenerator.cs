using CoffeeSql.Core.CodeFirst;
using CoffeeSql.Core.EntityDesign.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Mysql.CodeFirst
{
    public class MysqlDDLTextGenerator : DDLTextGeneratorBase
    {
        public MysqlDDLTextGenerator()
        {
            this.CREATE_TABLE_AtLast = "ENGINE=InnoDB DEFAULT CHARSET=utf8";
        }

        protected override string DropTableIfExist(string tableName)
        {
            return $"DROP TABLE IF EXISTS {tableName}";
        }

        protected override string TableName(Type tEntityType)
        {
            return $"{TableAttribute.GetName(tEntityType)}";
        }
    }
}
