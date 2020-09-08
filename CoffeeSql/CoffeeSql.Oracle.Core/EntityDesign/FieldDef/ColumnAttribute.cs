using CoffeeSql.Core.EntityDesign.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Oracle.Core.EntityDesign.FieldDef
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
