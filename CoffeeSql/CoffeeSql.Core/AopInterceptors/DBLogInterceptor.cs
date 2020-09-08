using System;
using System.Collections.Generic;
using System.Text;
using Castle.DynamicProxy;
using CoffeeSql.Core.DbContexts;

namespace CoffeeSql.Core.AopInterceptors
{
    /// <summary>
    /// Castle.Core Interceptor for Database Query Execution Log
    /// </summary>
    internal class DBLogInterceptor : StandardInterceptor
    {
        protected override void PostProceed(IInvocation invocation)
        {
            SqlDbContext dbContext = invocation.GetArgumentValue(0) as SqlDbContext;
            
            //Log Information about current Execution
            dbContext.Log(dbContext);
            base.PostProceed(invocation);
        }
    }
}
