using CoffeeSql.Core.EntityDesign.Attributes;
using CoffeeSql.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CoffeeSql.Core.SqlStatementManagement
{
    internal class LambdaToSql
    {
        internal static string ParametricFlag { private get; set; } = "@";

        public static string ConvertWhere<T>(Expression<Func<T, bool>> where) where T : class
        {
            IDictionary<string, object> parameters = new Dictionary<string, object>();

            StringBuilder builder = new StringBuilder();
            builder.Append(" WHERE ");
            if (where.Body is BinaryExpression)
            {
                BinaryExpression be = where.Body as BinaryExpression;

                return builder.Append(BinaryExpressionProvider(be.Left, be.Right, be.NodeType, ref parameters)).ToString();
            }

            return builder.Append(ExpressionRouter(where.Body, ref parameters)).ToString();

        }

        public static string ConvertWhere<T>(Expression<Func<T, bool>> where, IDictionary<string, object> parameters) where T : class
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(" WHERE ");
            if (where.Body is BinaryExpression be)
            {
                return builder.Append(BinaryExpressionProvider(be.Left, be.Right, be.NodeType, ref parameters)).ToString();
            }
            return builder.Append(ExpressionRouter(where.Body, ref parameters)).ToString();
        }

        //转换查询列
        public static List<string> ConvertColumns<TEntity>(Expression<Func<TEntity, object>> columns) where TEntity : class
        {
            List<string> strList = new List<string>();

            if (columns == null)  //默认查询所有字段
            {
                strList.Add("*");
                return strList;
            }
            
            if (columns.Body is NewExpression)
            {
                var newExp = columns.Body as NewExpression;
                foreach (var nExp in newExp.Arguments)
                {
                    strList.Add(GetFieldAttribute(nExp));
                }
            }
            else
            {
                strList.Add(GetFieldAttribute(columns.Body));
            }
            return strList;
        }

        public static string ConvertOrderBy<T>(Expression<Func<T, object>> orderby) where T : class
        {
            IDictionary<string, object> parameters = new Dictionary<string, object>();

            if (orderby.Body is UnaryExpression ue)
            {
                return ExpressionRouter(ue.Operand, ref parameters);
            }
            else
            {
                MemberExpression order = (MemberExpression)orderby.Body;
                return order.Member.Name;
            }
        }

        //通过Attribute获取需要查找的字段列表
        private static string GetFieldAttribute(Expression exp)
        {
            if (exp is UnaryExpression)
            {
                var ue = exp as UnaryExpression;
                return GetFieldAttribute(ue.Operand);
            }
            else if (exp is MemberExpression)
            {
                var mem = exp as MemberExpression;
                var member = mem.Member;
                var metaFieldAttr = member.GetCustomAttributes(typeof(ColumnBaseAttribute), true)?.FirstOrDefault();
                var metaFieldName = (metaFieldAttr as ColumnBaseAttribute)?.Name ?? member.Name;
                return metaFieldName;
            }
            else
            {
                MemberExpression order = (MemberExpression)exp;
                return GetFieldAttribute(order);
            }
        }

        private static string BinaryExpressionProvider(Expression left, Expression right, ExpressionType type, ref IDictionary<string, object> parameters)
        {
            var leftValue = ExpressionRouter(left, ref parameters);
            var typeCast = ExpressionTypeCast(type);
            var rightValue = ExpressionRouter(right, ref parameters);

            if (left is MemberExpression && (right is ConstantExpression || right is MemberExpression))
            {
                var keyNameNoPoint = leftValue.Replace(".", "");

                keyNameNoPoint = FindAppropriateKey($"{ParametricFlag}{keyNameNoPoint}", parameters);

                parameters.AddOrUpdate(keyNameNoPoint, $"{rightValue}");

                return $"{leftValue} {typeCast} {keyNameNoPoint}";
            }
            else
            {
                return $"({leftValue} {typeCast} {rightValue})";
            }

        }

        private static string ExpressionRouter(Expression exp, ref IDictionary<string, object> parameters)
        {
            if (exp is BinaryExpression)
            {
                BinaryExpression be = exp as BinaryExpression;

                return BinaryExpressionProvider(be.Left, be.Right, be.NodeType, ref parameters);
            }
            else if (exp is MemberExpression)
            {
                MemberExpression me = exp as MemberExpression;

                if (!exp.ToString().StartsWith("value"))
                {
                    //return me.ToString();
                    return $"{me.ToString().Split('.')[0]}.{GetFieldAttribute(me)}";
                }
                else
                {
                    var result = Expression.Lambda(exp).Compile().DynamicInvoke();
                    if (result == null)
                    {
                        return "NULL";
                    }
                    else if (result is ValueType)
                    {
                        if (result is Guid)
                        {
                            return $"'{result}'";
                        }
                        return result.ToString();
                    }
                    else if (result is string || result is DateTime || result is char)
                    {
                        return $"{result}";
                    }
                }
            }
            else if (exp is NewArrayExpression)
            {
                NewArrayExpression ae = exp as NewArrayExpression;

                StringBuilder tmpstr = new StringBuilder();
                foreach (Expression ex in ae.Expressions)
                {
                    tmpstr.Append(ExpressionRouter(ex, ref parameters));
                    tmpstr.Append(",");
                }

                return tmpstr.ToString(0, tmpstr.Length - 1);
            }
            else if (exp is MethodCallExpression)
            {
                MethodCallExpression mce = exp as MethodCallExpression;

                //get value
                string value = null;
                if (mce.Object == null)
                {
                    value = Expression.Lambda(mce).Compile().DynamicInvoke().ToString();
                }
                else
                {
                    value = Expression.Lambda(mce.Arguments[0]).Compile().DynamicInvoke().ToString();
                }

                //参数名
                var keyName = $"{mce.Object.ToString().Split('.')[0]}.{GetFieldAttribute(mce.Object)}";//mce.Object.ToString();
                var keyNameNoPoint = keyName.Replace(".", "");

                keyNameNoPoint = FindAppropriateKey($"{ParametricFlag}{keyNameNoPoint}", parameters);

                if (mce.Method.Name.Equals("Equals"))
                {
                    parameters.AddOrUpdate(keyNameNoPoint, $"{value}");
                    return $"{keyName} = {keyNameNoPoint}";
                }
                else if (mce.Method.Name.Equals("Contains"))
                {
                    parameters.AddOrUpdate(keyNameNoPoint, $"%{value.Replace("'", "")}%");
                    return $"{keyName} LIKE {keyNameNoPoint}";
                }
                else if (mce.Method.Name.Equals("StartsWith"))
                {
                    parameters.AddOrUpdate(keyNameNoPoint, $"{value.Replace("'", "")}%");
                    return $"{keyName} LIKE {keyNameNoPoint}";
                }
                else if (mce.Method.Name.Equals("EndsWith"))
                {
                    parameters.AddOrUpdate(keyNameNoPoint, $"%{value.Replace("'", "")}");
                    return $"{keyName} LIKE {keyNameNoPoint}";
                }

                return value;

            }
            else if (exp is ConstantExpression)
            {
                ConstantExpression ce = exp as ConstantExpression;

                if (ce.Value == null)
                {
                    return "NULL";
                }
                else if (ce.Value is ValueType)
                {
                    if (ce.Value is bool)
                    {
                        bool b = (bool)ce.Value;

                        if (b)
                        {
                            return " 1=1 ";
                        }
                        else
                        {
                            return " 1=2 ";
                        }
                    }

                    return ce.Value.ToString();
                }
                else if (ce.Value is string)
                {
                    return ce.Value.ToString();
                }



            }

            return null;
        }

        private static string ExpressionTypeCast(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.Equal:
                    return " = ";
                case ExpressionType.GreaterThan:
                    return " > ";
                case ExpressionType.GreaterThanOrEqual:
                    return " >= ";
                case ExpressionType.LessThan:
                    return " < ";
                case ExpressionType.LessThanOrEqual:
                    return " <= ";
                case ExpressionType.NotEqual:
                    return " <> ";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return " Or ";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return " + ";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return " - ";
                case ExpressionType.Divide:
                    return " / ";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return " * ";
                default:
                    return null;

            }

        }

        private static string FindAppropriateKey(string key, IDictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey(key))
            {
                return key;
            }

            //循环99次，如果期间有符合条件的直接返回
            for (int i = 0; i < 99; i++)
            {
                string tempKey = $"{key}{i}";
                if (!parameters.ContainsKey(tempKey))
                {
                    return tempKey;
                }

            }

            throw new KeyNotFoundException($"The appropriate key was not found in the interval [{key}0,{key}99]");
        }

    }
}
