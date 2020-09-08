using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using CoffeeSql.Core.EntityDesign.Attributes;

namespace CoffeeSql.Core.SqlDataAccess
{
    /// <summary>
    /// Auto Fill Adapter
    /// => Fill DataRow to Entity
    /// </summary>
    public class EntityFillAdapter<Entity>
    {
        private static readonly Func<DataRow, Entity> funcCache = GetFactory();

        public static Entity AutoFill(DataRow row)
        {
            return funcCache(row);
        }

        private static Func<DataRow, Entity> GetFactory()
        {
            #region get Info through Reflection
            var entityType = typeof(Entity);
            var rowType = typeof(DataRow);
            var convertType = typeof(Convert);
            var typeType = typeof(Type);
            var columnCollectionType = typeof(DataColumnCollection);
            var getTypeMethod = typeType.GetMethod("GetType", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null);
            var changeTypeMethod = convertType.GetMethod("ChangeType", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(object), typeof(Type) }, null);
            var containsMethod = columnCollectionType.GetMethod("Contains");
            var rowIndexerGetMethod = rowType.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(string) }, new[] { new ParameterModifier(1) });
            var columnCollectionIndexerGetMethod = columnCollectionType.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(int) }, new[] { new ParameterModifier(1) });
            var entityIndexerSetMethod = entityType.GetMethod("set_Item", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(object) }, null);
            var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            #endregion

            #region some Expression class that can be repeat used
            //DataRow row
            var rowDeclare = Expression.Parameter(rowType, "row");
            //Student entity
            var entityDeclare = Expression.Parameter(entityType, "entity");
            //Type propertyType
            var propertyTypeDeclare = Expression.Parameter(typeof(Type), "propertyType");
            //new Student()
            var newEntityExpression = Expression.New(entityType);
            //row == null
            var rowEqualnullExpression = Expression.Equal(rowDeclare, Expression.Constant(null));
            //row.Table.Columns
            var rowTableColumns = Expression.Property(Expression.Property(rowDeclare, "Table"), "Columns");
            //int loopIndex
            var loopIndexDeclare = Expression.Parameter(typeof(int), "loopIndex");
            //row.Table.Columns[loopIndex].ColumnName
            var columnNameExpression = Expression.Property(Expression.Call(rowTableColumns, columnCollectionIndexerGetMethod, loopIndexDeclare), "ColumnName");
            //break;
            LabelTarget labelBreak = Expression.Label();
            //default(Student)
            var defaultEntityValue = Expression.Default(entityType);
            #endregion

            var setRowNotNullBlockExpressions = new List<Expression>();
                        
            #region entity = new Student();loopIndex = 0;
            setRowNotNullBlockExpressions.Add(Expression.Assign(entityDeclare, newEntityExpression));
            setRowNotNullBlockExpressions.Add(Expression.Assign(loopIndexDeclare, Expression.Constant(0)));

            #endregion

            #region loop Fill DataRow's field to Entity Indexer
            /*
             * while (true)
             * {
             *     if (loopIndex < row.Table.Columns.Count)
             *     {
             *         entity[row.Table.Columns[loopIndex].ColumnName] = row[row.Table.Columns[loopIndex].ColumnName];
             *         loopIndex++;
             *     }
             *     else break;
             * } 
             */

            setRowNotNullBlockExpressions.Add(

                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(loopIndexDeclare, Expression.Property(rowTableColumns, "Count")),
                        Expression.Block(
                            Expression.Call(entityDeclare, entityIndexerSetMethod, columnNameExpression, Expression.Call(rowDeclare, rowIndexerGetMethod, columnNameExpression)),
                            Expression.PostIncrementAssign(loopIndexDeclare)
                        ),
                        Expression.Break(labelBreak)
                    ),
                    labelBreak
                )
            );
            #endregion

            #region assign for Entity property
            foreach (var propertyInfo in properties)
            {
                var columnAttr = propertyInfo.GetCustomAttribute(typeof(ColumnBaseAttribute), true) as ColumnBaseAttribute;

                // no column , no translation
                if (null == columnAttr) continue;

                if (propertyInfo.CanWrite)
                {
                    var columnName = Expression.Constant(columnAttr.GetName(propertyInfo.Name), typeof(string));

                    //entity.Id
                    var propertyExpression = Expression.Property(entityDeclare, propertyInfo);
                    //row["Id"]
                    var value = Expression.Call(rowDeclare, rowIndexerGetMethod, columnName);
                    //default(string)
                    var defaultValue = Expression.Default(propertyInfo.PropertyType);
                    //row.Table.Columns.Contains("Id")
                    var checkIfContainsColumn = Expression.Call(rowTableColumns, containsMethod, columnName);
                    //!row["Id"].Equals(DBNull.Value)
                    var checkDBNull = Expression.NotEqual(value, Expression.Constant(System.DBNull.Value));
                    
                    var propertyTypeName = Expression.Constant(propertyInfo.PropertyType.ToString(), typeof(string));

                    /*
                     * if (row.Table.Columns.Contains("Id") && !row["Id"].Equals(DBNull.Value))
                     * {
                     *     propertyType = Type.GetType("System.String");
                     *     entity.Id = (string)Convert.ChangeType(row["Id"], propertyType);
                     * }
                     * else
                     *     entity.Id = default(string);
                     */
                    setRowNotNullBlockExpressions.Add(

                        Expression.IfThenElse(
                            Expression.AndAlso(checkIfContainsColumn, checkDBNull),
                            Expression.Block(
                                Expression.Assign(propertyTypeDeclare, Expression.Call(getTypeMethod, propertyTypeName)),
                                Expression.Assign(propertyExpression, Expression.Convert(Expression.Call(changeTypeMethod, value, propertyTypeDeclare), propertyInfo.PropertyType))
                            ),
                            Expression.Assign(propertyExpression, defaultValue)
                        )
                    );
                }
            }

            #endregion

            var checkIfRowIsNull = Expression.IfThenElse(
                rowEqualnullExpression,
                Expression.Assign(entityDeclare, defaultEntityValue),
                Expression.Block(setRowNotNullBlockExpressions)
            );

            var body = Expression.Block(

                new[] { entityDeclare, loopIndexDeclare, propertyTypeDeclare },
                checkIfRowIsNull,
                entityDeclare   //return Student;
            );

            return Expression.Lambda<Func<DataRow, Entity>>(body, rowDeclare).Compile();
        }
    }

    #region
    //public class Student : EntityDesign.EntityBase
    //{
    //    [Column]
    //    public string Id { get; set; }

    //    [Column("StudentName")]
    //    public string Name { get; set; }
    //}
    ////this is the template of "GetFactory()" created.
    //public static Student StudentFillAdapter(DataRow row)
    //{
    //    Student entity;
    //    int loopIndex;
    //    Type propertyType;

    //    if (row == null)
    //        entity = default(Student);
    //    else
    //    {
    //        entity = new Student();
    //        loopIndex = 0;

    //        while (true)
    //        {
    //            if (loopIndex < row.Table.Columns.Count)
    //            {
    //                entity[row.Table.Columns[loopIndex].ColumnName] = row[row.Table.Columns[loopIndex].ColumnName];
    //                loopIndex++;
    //            }
    //            else break;
    //        }

    //        if (row.Table.Columns.Contains("Id") && !row["Id"].Equals(DBNull.Value))
    //        {
    //            propertyType = Type.GetType("System.String");
    //            entity.Id = (string)Convert.ChangeType(row["Id"], propertyType);
    //        }
    //        else
    //            entity.Id = default(string);

    //        if (row.Table.Columns.Contains("StudentName") && !row["StudentName"].Equals(DBNull.Value))
    //        {
    //            propertyType = Type.GetType("System.String");
    //            entity.Name = (string)Convert.ChangeType(row["StudentName"], propertyType);
    //        }
    //        else
    //            entity.Name = default(string);
    //    }

    //    return entity;
    //}
    #endregion
}
