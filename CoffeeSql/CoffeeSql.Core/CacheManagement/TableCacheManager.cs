﻿using CoffeeSql.Core.EntityDesign.Attributes;
using CoffeeSql.Core.Configs;
using CoffeeSql.Core.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeSql.Core.CacheManagement
{
    /// <summary>
    /// 表缓存管理器(二级缓存管理器）
    /// </summary>
    public class TableCacheManager : CacheManagerBase
    {
        internal TableCacheManager(DbContext context) : base(context) { }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public void FlushAllCache()
        {
            if (CacheStorageManager.IsExist(CoffeeSqlConst.GetTableCacheKeysCacheKey(DbContext.DataBaseName), out HashSet<string> keys))
            {
                foreach (var item in keys)
                {
                    CacheStorageManager.Delete(item);
                }
            }
        }

        /// <summary>
        /// 清空单个表相关的所有缓存
        /// </summary>
        /// <param name="dbContext"></param>
        public void FlushCollectionCache(string collectionName = null)
        {
            CacheStorageManager.Delete(GetTableCacheKey(collectionName));
        }

        private string GetTableCacheKey(string collectionName = null)
        {
            string key = $"{CoffeeSqlConst.CacheKey_TableCache}{collectionName ?? DbContext.CollectionName}";
            //缓存键更新
            if (!CacheStorageManager.IsExist(CoffeeSqlConst.GetTableCacheKeysCacheKey(DbContext.DataBaseName), out HashSet<string> keys))
            {
                keys = new HashSet<string>();
            }
            keys.Add(key);
            CacheStorageManager.Put(CoffeeSqlConst.GetTableCacheKeysCacheKey(DbContext.DataBaseName), keys, DbContext.MaxExpiredTimeSpan);
            return key;
        }

        /// <summary>
        /// 更新数据到缓存（Add）
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entity"></param>
        internal void AddCache<TEntity>(TEntity entity)
        {
            if (DbContext.OpenTableCache)
            {
                var tableName = TableAttribute.GetName(typeof(TEntity));
                //如果存在表级别缓存，则更新数据到缓存
                if (CacheStorageManager.IsExist(GetTableCacheKey(tableName), out List<TEntity> entities))
                {
                    if (TableCachingAttribute.IsExistTaleCaching(typeof(TEntity), out TimeSpan tableCacheTimeSpan))
                    {
                        entities.Add(entity);
                        //如果过期时间为0，则取上下文的过期时间
                        CacheStorageManager.Put(GetTableCacheKey(tableName), entities, tableCacheTimeSpan == TimeSpan.Zero ? DbContext.TableCacheExpiredTimeSpan : tableCacheTimeSpan);
                    }
                }
            }
        }
        /// <summary>
        /// 更新数据到缓存（Add）
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="values"></param>
        internal void AddCache<TEntity>(IEnumerable<TEntity> values)
        {
            if (DbContext.OpenTableCache)
            {
                var tableName = TableAttribute.GetName(typeof(TEntity));
                //如果存在表级别缓存，则更新数据到缓存
                if (CacheStorageManager.IsExist(GetTableCacheKey(tableName), out List<TEntity> entities))
                {
                    if (TableCachingAttribute.IsExistTaleCaching(typeof(TEntity), out TimeSpan tableCacheTimeSpan))
                    {
                        //如果过期时间为0，则取上下文的过期时间
                        TimeSpan timeSpan = tableCacheTimeSpan == TimeSpan.Zero ? DbContext.TableCacheExpiredTimeSpan : tableCacheTimeSpan;

                        entities.AddRange(values);
                        CacheStorageManager.Put(GetTableCacheKey(tableName), entities, tableCacheTimeSpan);
                    }
                }
            }
        }
        /// <summary>
        /// 更新数据到缓存（Update）
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entity"></param>
        /// <param name="filter"></param>
        internal void UpdateCache<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> filter, IEnumerable<string> updateCloumns = null)
        {
            if (DbContext.OpenTableCache)
            {
                var tableName = TableAttribute.GetName(typeof(TEntity));
                //如果存在表级别缓存，则更新数据到缓存
                if (CacheStorageManager.IsExist(GetTableCacheKey(tableName), out List<TEntity> oldEntities))
                {
                    if (TableCachingAttribute.IsExistTaleCaching(typeof(TEntity), out TimeSpan tableCacheTimeSpan))
                    {
                        //如果过期时间为0，则取上下文的过期时间
                        TimeSpan timeSpan = tableCacheTimeSpan == TimeSpan.Zero ? DbContext.TableCacheExpiredTimeSpan : tableCacheTimeSpan;
                        //从缓存集合中寻找该记录，如果找到，则更新该记录
                        var list = oldEntities.Where(filter.Compile()).ToList();
                        if (list != null && list.Any())
                        {
                            List<TEntity> newEntities;

                            oldEntities.RemoveAll(t => list.Contains(t));

                            if (null == updateCloumns)
                            {
                                //改变了多条，更新对应字段
                                newEntities = UpdateEntitiesField(oldEntities, entity, updateCloumns);
                            }
                            else
                            {
                                //只改变了传入的唯一一条
                                newEntities = oldEntities;
                                newEntities.Add(entity);
                            }

                            CacheStorageManager.Put(GetTableCacheKey(tableName), newEntities, timeSpan);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 更新数据到缓存（Delete）
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="filter"></param>
        internal void DeleteCache<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            if (DbContext.OpenTableCache)
            {
                var tableName = TableAttribute.GetName(typeof(TEntity));
                //如果存在表级别缓存，则更新数据到缓存
                if (CacheStorageManager.IsExist(GetTableCacheKey(tableName), out List<TEntity> entities))
                {

                    if (TableCachingAttribute.IsExistTaleCaching(typeof(TEntity), out TimeSpan tableCacheTimeSpan))
                    {
                        //从缓存集合中寻找该记录，如果找到，则更新该记录
                        var list = entities.Where(filter.Compile()).ToList();
                        if (list != null && list.Any())
                        {
                            entities.RemoveAll(t => list.Contains(t));
                            //如果过期时间为0，则取上下文的过期时间
                            CacheStorageManager.Put(GetTableCacheKey(tableName), entities, tableCacheTimeSpan == TimeSpan.Zero ? DbContext.TableCacheExpiredTimeSpan : tableCacheTimeSpan);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 更新数据到缓存（Delete）
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entity"></param>
        internal void DeleteCache<TEntity>(TEntity entity)
        {
            if (DbContext.OpenTableCache)
            {
                var tableName = TableAttribute.GetName(typeof(TEntity));
                //如果存在表级别缓存，则更新数据到缓存
                if (CacheStorageManager.IsExist(GetTableCacheKey(tableName), out List<TEntity> entities))
                {

                    if (TableCachingAttribute.IsExistTaleCaching(typeof(TEntity), out TimeSpan tableCacheTimeSpan))
                    {
                        //如果过期时间为0，则取上下文的过期时间
                        TimeSpan timeSpan = tableCacheTimeSpan == TimeSpan.Zero ? DbContext.TableCacheExpiredTimeSpan : tableCacheTimeSpan;
                        //从缓存集合中寻找该记录，如果找到，则更新该记录
                        var val = entities.Find(t => t.Equals(entity));
                        if (val != null)
                        {
                            entities.Remove(val);
                            CacheStorageManager.Put(GetTableCacheKey(tableName), entities, tableCacheTimeSpan);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 从缓存中获取数据，如果没有，则后台执行扫描表任务
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        internal List<TEntity> GetEntitiesFromCache<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class
        {
            //1.检查是否开启了Table缓存
            if (!DbContext.OpenTableCache)
            {
                return null;
            }

            //2.如果TableCache里面有该缓存键，则直接获取
            if (CacheStorageManager.IsExist(GetTableCacheKey(TableAttribute.GetName(typeof(TEntity))), out List<TEntity> entities))
            {
                DbContext.IsFromCache = true;
                return entities.Where(filter.Compile()).ToList();
            }

            //3.则判断是否需要对该表进行扫描（含有TableCachingAttribute的标记的类才可以有扫描全表的权限）
            if (TableCachingAttribute.IsExistTaleCaching(typeof(TEntity), out TimeSpan tableCacheTimeSpan))
            {
                //执行扫描全表数据任务
                ScanTableBackground<TEntity>(tableCacheTimeSpan);
            }

            return null;
        }

        private readonly object tableScaningLocker = new object();
        /// <summary>
        /// 后台扫描全表数据
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext">上下文</param>
        /// <param name="tableCacheTimeSpan">tableCache过期时间</param>
        private void ScanTableBackground<TEntity>(TimeSpan tableCacheTimeSpan) where TEntity : class
        {
            string scanKey = $"{CoffeeSqlConst.CacheKey_TableScanning}{DbContext.CollectionName}";
            //1.判断正在扫描键是否存在，如果存在，则返回null，继续等待扫描任务完成
            if (CacheStorageManager.IsExist(scanKey))
            {
                return;
            }
            //2.如果没有扫描键，则执行后台扫描任务
            Task.Run(() =>
            {
                //设置扫描键，标识当前正在进行扫描
                CacheStorageManager.Put(scanKey, 1, CoffeeSqlConst.SpanScaningKeyExpiredTime);
                //对扫描任务加锁，防止多线程环境多次执行任务
                lock (tableScaningLocker)
                {
                    var tableName = TableAttribute.GetName(typeof(TEntity));
                    //双重校验当前缓存是否存在TableCache，防止多个进程在锁外等待，所释放后再次执行
                    if (CacheStorageManager.IsExist(GetTableCacheKey(tableName)))
                    {
                        return;
                    }
                    //如果过期时间为0，则取上下文的过期时间
                    TimeSpan timeSpan = tableCacheTimeSpan == TimeSpan.Zero ? DbContext.TableCacheExpiredTimeSpan : tableCacheTimeSpan;
                    //执行扫描全表任务，并将结果存入缓存中
                    var data = DbContext.GetFullCollectionData<TEntity>();
                    if (data != null)
                    {
                        CacheStorageManager.Put(GetTableCacheKey(tableName), data, DbContext.TableCacheExpiredTimeSpan);
                    }
                }
                //将扫描键移除，表示已经扫描完成
                CacheStorageManager.Delete(scanKey);
            });
        }

        /// <summary>
        /// 更新实体对象集合的指定列名的字段值
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="oldEntities"></param>
        /// <param name="newValue">要更新的值</param>
        /// <param name="updateCloumns">要更新的列名集合</param>
        /// <returns></returns>
        private List<TEntity> UpdateEntitiesField<TEntity>(List<TEntity> oldEntities, TEntity newValue, IEnumerable<string> updateCloumns)
        {
            PropertyInfo[] propertyInfos = typeof(TEntity).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            List<TEntity> newEntities = oldEntities.Select(oldEntity =>
            {
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    var columnAttr = propertyInfo.GetCustomAttribute(typeof(ColumnBaseAttribute), true) as ColumnBaseAttribute;

                    if (null == columnAttr) continue;
                    
                    string columnName = columnAttr.GetName(propertyInfo.Name);

                    if (updateCloumns.Contains(columnName))
                    {
                        propertyInfo.SetValue(oldEntity, propertyInfo.GetValue(newValue));
                    }
                }

                return oldEntity;
            }).ToList();

            return newEntities;
        }
    }
}
