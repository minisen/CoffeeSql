using CoffeeSql.Core.EntityDesign.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Oracle.Core.EntityDesign.FieldDef
{
    public class AutoIncreaseAttribute : AutoIncreaseBaseAttribute
    {
        public AutoIncreaseAttribute()
        {
            this.SortNum = (int)FieldDefSORT.AUTOINCREASE;
        }

    }
}
