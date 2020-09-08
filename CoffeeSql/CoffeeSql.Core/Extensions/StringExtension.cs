using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeSql.Core.Extensions
{
    internal static class StringExtension
    {
        /// <summary>
        /// Format the original connection string as a rule 
        ///     rule => Parameter Key toUpper,Order by Key ASC and Split by ';'
        /// </summary>
        /// <param name="originalConnStr"></param>
        /// <returns></returns>
        public static string ToFormativeConnectionString(this string originalConnStr)
        {
            if (string.IsNullOrEmpty(originalConnStr))
            {
                return originalConnStr;
            }

            Dictionary<string, string> connParamDir = new Dictionary<string, string>();

            originalConnStr.Trim(' ', ';').Split(';').Foreach(s =>
            {
                string key = s.Substring(0, s.IndexOf('=')).Trim().ToUpper();
                string value = s.Substring(s.IndexOf('=') + 1);

                connParamDir.Add(key, value);
            });

            //order by Key ASC
            connParamDir = connParamDir.OrderBy(kv => kv.Key).ToDictionary(o => o.Key, p => p.Value);

            string[] connParams = connParamDir.Select(dir => $"{dir.Key}={dir.Value}").ToArray();

            return $"{string.Join(";", connParams)};";
        }
    }
}
