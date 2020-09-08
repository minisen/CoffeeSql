using CoffeeSql.Core.EntityDesign.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Mysql.EntityDesign.FieldDef
{
    public class FieldTypeDefBase : FieldDefBaseAttribute
    {
        public int? Length { get; set; } = null;
        public FieldTypeDefBase()
        {
            this.SortNum = (int)FieldDefSORT.FIELDTYPE;
        }

        public FieldTypeDefBase(int length)
        {
            this.SortNum = (int)FieldDefSORT.FIELDTYPE;
            this.Length = length;
        }
    }
}
