﻿using CoffeeSql.Core.CodeFirst.Interface;
using CoffeeSql.Core.EntityDesign.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CoffeeSql.Oracle.Core.EntityDesign.FieldDef
{
    public class PrimaryKeyAttribute : PrimaryKeyBaseAttribute, ICodeFirstFieldDef
    {
        public PrimaryKeyAttribute()
        {
            this.SortNum = (int)FieldDefSORT.PRIMARYKEY;
        }

        public string getFieldDefDDLItem(PropertyInfo fieldPropertyInfo = null)
        {
            return "PRIMARY KEY";
        }
    }
}
