using CoffeeSql.Core.CodeFirst;
using CoffeeSql.Core.EntityDesign.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeSql.Oracle.CodeFirst
{
    public class OracleDDLTextGenerator : DDLTextGeneratorBase
    {
        protected override string DropTableIfExist(string tableName)
        {
            string dropText =
                $"declare\r\n" +
                $"    v_count number;\r\n" +
                $"begin\r\n" +
                $"    select count(1) into v_count from user_tables t where t.TABLE_NAME='{tableName}';\r\n" +
                $"if v_count>0 then\r\n" +
                $"    execute immediate'drop table {tableName}';\r\n" +
                $"end if;\r\n" +
                $"end;\r\n";

            return dropText;
        }

        protected override string TableName(Type tEntityType)
        {
            return $"{TableAttribute.GetName(tEntityType).ToUpper()}";
        }
    }
}
