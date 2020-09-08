using CoffeeSql.Core.CodeFirst.Interface;
using CoffeeSql.Core.EntityDesign.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeSql.Oracle.EntityDesign.FieldDef
{
    public class PrimaryKeyAttribute : PrimaryKeyBaseAttribute, ICodeFirstFieldDef
    {
        public PrimaryKeyAttribute()
        {
            this.SortNum = (int)FieldDefSORT.PRIMARYKEY;
        }

        public string getFieldDefDDLItem(object param = null)
        {
            return "PRIMARY KEY";
        }
    }
}
