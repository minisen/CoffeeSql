using CoffeeSql.Core.EntityDesign.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeSql.Oracle.EntityDesign.FieldDef
{
    public class AutoIncreaseAttribute : AutoIncreaseBaseAttribute
    {
        public AutoIncreaseAttribute()
        {
            this.SortNum = (int)FieldDefSORT.AUTOINCREASE;
        }

    }
}
