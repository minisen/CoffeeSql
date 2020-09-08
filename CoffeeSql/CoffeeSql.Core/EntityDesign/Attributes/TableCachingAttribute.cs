using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeSql.Core.EntityDesign.Attributes
{
    /// <summary>
    /// Mark this Attribute for Entity Class which datatable need be cached
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TableCachingAttribute : Attribute
    {
        public TimeSpan ExpiredTime { get; private set; }
        public TableCachingAttribute() { }
        public TableCachingAttribute(int expiredTimeMinutes)
        {
            ExpiredTime = TimeSpan.FromMinutes(expiredTimeMinutes);
        }

        public static bool IsExistTaleCaching(Type type, out TimeSpan timeSpan)
        {
            var attr = type.GetCustomAttributes(typeof(TableCachingAttribute), true)?.FirstOrDefault();
            timeSpan = (attr as TableCachingAttribute)?.ExpiredTime ?? TimeSpan.Zero;//这里默认给Zero，在TableCache里面判断Zero则获取Context的默认值
            if (attr == null)
            {
                return false;
            }
            return true;
        }
    }
}
