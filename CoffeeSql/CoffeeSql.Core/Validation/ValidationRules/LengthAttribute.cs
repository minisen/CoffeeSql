using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CoffeeSql.Core.Validation.ValidationRules
{
    /// <summary>
    /// Attribute of property value's length validation
    /// </summary>
    public class LengthAttribute : ValidationAttribute
    {
        internal int LengthMin { get; set; }
        internal int LengthMax { get; set; }

        public LengthAttribute(int lengthMin = 0, int lengthMax = int.MaxValue, string errorMsg = null) : base(errorMsg)
        {
            this.LengthMin = lengthMin;
            this.LengthMax = lengthMax;
        }

        public override void Verify(PropertyInfo propertyInfo, object value)
        {
            if (!(value is string))
            {
                throw new ArgumentException($"LengthAttribute（数据验证标签）只有标记在类型为 string 的属性上有效");
            }

            string stringValue = value as string;

            if (stringValue.Length < LengthMin || stringValue.Length > LengthMax)
            {
                throw new ArgumentOutOfRangeException(this.ErrorMessage ?? $"length of '{propertyInfo.Name}' is out of range:[{LengthMin},{LengthMax}], parameter value:{value}");
            }

        }
    }
}
