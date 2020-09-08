using CoffeeSql.Core.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeSql.Core.Validation
{
    /// <summary>
    /// Field value validity verifier
    /// </summary>
    internal class PropertyDataValidator
    {
        /// <summary>
        /// Validation method of field value validity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        public static void Verify<TEntity>(DbContext context, TEntity entity) where TEntity : class
        {

            //判断是否开启字段值校验
            if (!context.OpenPropertyDataValidate)
            {
                return;
            }


            if (null == entity)
            {
                return;
            }

            foreach (var propertyInfo in typeof(TEntity).GetProperties())
            {
                object value = propertyInfo.GetValue(entity);

                var validationAttributes = propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), false);

                foreach (var validationAttribute in validationAttributes)
                {
                    ValidationAttribute attr = validationAttribute as ValidationAttribute;

                    attr.Verify(propertyInfo, value);
                }
            }
        }
    }
}
