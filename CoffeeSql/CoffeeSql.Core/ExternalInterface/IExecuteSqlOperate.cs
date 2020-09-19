using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeSql.Core.ExternalInterface
{
    /// <summary>
    /// 执行SQL语句扩展Api
    /// </summary>
    public interface IExecuteSqlOperate
    {
        int ExecuteSql(string sqlStatement, params object[] parms);
        Task<int> ExecuteSqlAsync(string sqlStatement, params object[] parms);
    }
}
