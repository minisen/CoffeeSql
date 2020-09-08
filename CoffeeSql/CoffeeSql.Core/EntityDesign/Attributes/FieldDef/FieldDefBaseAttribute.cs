using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.EntityDesign.Attributes
{
    /// <summary>
    /// All table field constraints attributes must inherit from this basic class
    /// </summary>
    public abstract class FieldDefBaseAttribute : Attribute
    {
        /// <summary>
        /// Sort Number when generating DDL of field define
        /// </summary>
        public int SortNum { get; set; } = 0;
    }
}
