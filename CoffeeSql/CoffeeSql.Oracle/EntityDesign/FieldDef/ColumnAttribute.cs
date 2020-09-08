using CoffeeSql.Core.EntityDesign.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeSql.Oracle.EntityDesign.FieldDef
{
    public class ColumnAttribute : ColumnBaseAttribute
    {
        public ColumnAttribute() : base()
        {
            this.SortNum = (int)FieldDefSORT.CLOUMN;
        }
        public ColumnAttribute(string columnName) : base(columnName)
        {
            this.SortNum = (int)FieldDefSORT.CLOUMN;
        }
    }
}
