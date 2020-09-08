using CoffeeSql.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using CoffeeSql.Core.Extensions;

namespace CoffeeSql.Core.EntityDesign
{
    /// <summary>
    /// the base class of Entity
    /// </summary>
    public class EntityBase
    {
        /// <summary>
        /// storage QueryField's Result
        /// </summary>
        private Dictionary<string, object> queryFieldsDictionary = new Dictionary<string, object>();

        /// <summary>
        /// the Indexer of QueryField Result
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns>field value</returns>
        public object this[string fieldName]
        {
            get
            {
                if (this.queryFieldsDictionary.ContainsKey(fieldName))
                {
                    return this.queryFieldsDictionary[fieldName];
                }
                else
                {
                    throw new EntityBaseFieldNotExistException($"Field [{fieldName}] is not exist in this EntityBase Indexer.");
                }
            }

            internal set
            {
                this.queryFieldsDictionary.AddOrUpdate(fieldName, value);
            }
        }

        /// <summary>
        /// the count of QueryFields
        /// </summary>
        public int QueryFieldCount
        {
            get { return this.queryFieldsDictionary.Count; }
        }

    }
}
