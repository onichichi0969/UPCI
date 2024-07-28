using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;
using UPCI.DAL.DTO.Request;
using UPCI.DAL.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;

public class ExpressionBuilder
{
    private static MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
    private static MethodInfo startsWithMethod =
    typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
    private static MethodInfo endsWithMethod =
    typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });


    public static Expression<Func<T, bool>> GetExpression<T>(IList<Filter> filters)
    {
        if (filters == null || filters.Count == 0)
            return null;

        ParameterExpression param = Expression.Parameter(typeof(T), "t");
        Expression exp = null;

        foreach (var filter in filters)
        {
            var currentExpression = GetExpression(param, filter);
            exp = exp == null ? currentExpression : Expression.AndAlso(exp, currentExpression);
        }

        return Expression.Lambda<Func<T, bool>>(exp, param);
    }
    private static Expression GetExpression(ParameterExpression param, Filter filter)
    {
        MemberExpression member = Expression.Property(param, filter.Property);

        switch (filter.Operator.ToUpper())
        {
            case Operators.Equals:
                return Expression.Equal(member, ConvertValueType(member, filter.Value));
            case Operators.NotEquals:
                return Expression.NotEqual(member, ConvertValueType(member, filter.Value));
            case Operators.GreaterThan:
                return Expression.GreaterThan(member, ConvertValueType(member, filter.Value));
            case Operators.GreaterThanOrEqual:
                return Expression.GreaterThanOrEqual(member, ConvertValueType(member, filter.Value));
            case Operators.LessThan:
                return Expression.LessThan(member, ConvertValueType(member, filter.Value));
            case Operators.LessThanOrEqual:
                return Expression.LessThanOrEqual(member, ConvertValueType(member, filter.Value));
            case Operators.Contains:
                return Expression.Call(member, containsMethod, ConvertValueType(member, filter.Value));
            case Operators.StartsWith:
                return Expression.Call(member, startsWithMethod, ConvertValueType(member, filter.Value));
            case Operators.EndsWith:
                return Expression.Call(member, endsWithMethod, ConvertValueType(member, filter.Value));
            case Operators.Between:
                Expression lowerBound = ConvertValueType(member, filter.Value);
                Expression upperBound = ConvertValueType(member, filter.Value2);
                lowerBound = ConvertValueType(member, filter.Value);
                upperBound = ConvertValueType(member, filter.Value2);
                //if (member.Type == typeof(DateTime))
                //{
                //    lowerBound = ConvertValueType(member, Convert.ToDateTime(filter.Value).Date);
                //    upperBound = ConvertValueType(member, Convert.ToDateTime(filter.Value2).Date);
                //}
                //else 
                //{
                //    lowerBound = ConvertValueType(member, filter.Value);
                //    upperBound = ConvertValueType(member, filter.Value2);
                //}
                var lowerBoundExpression = Expression.GreaterThanOrEqual(member, lowerBound);
                var upperBoundExpression = Expression.LessThanOrEqual(member, upperBound); 
                return Expression.AndAlso(lowerBoundExpression, upperBoundExpression);
            case Operators.In:
                var values = filter.Values.Select(v => ConvertValueType(member, v)).ToArray();
                var inExpression = values.Select(v => Expression.Equal(member, v)).Aggregate<Expression>((acc, equal) => Expression.OrElse(acc, equal));
                return inExpression;
            case Operators.IsNull:
                return Expression.Equal(member, Expression.Constant(null, member.Type));
            case Operators.IsNotNull:
                return Expression.NotEqual(member, Expression.Constant(null, member.Type));
            default:
                throw new NotSupportedException($"The operator '{filter.Operator}' is not supported");
        }
    }
    
    
    private static Expression ConvertValueType(MemberExpression member, object value)
    {
        if (Nullable.GetUnderlyingType(member.Type) != null)
        {
            // Handle nullable types
            Type underlyingType = Nullable.GetUnderlyingType(member.Type);

            if (underlyingType == typeof(int))
            {
                return Expression.Constant(Convert.ToInt32(value), member.Type);
            }
            else if (underlyingType == typeof(double))
            {
                return Expression.Constant(Convert.ToDouble(value), member.Type);
            }
            else if (underlyingType == typeof(decimal))
            {
                return Expression.Constant(Convert.ToDecimal(value), member.Type);
            }
            else if (underlyingType == typeof(DateTime))
            { 
                DateTime date = DateTime.ParseExact(value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;
                return Expression.Constant(Convert.ChangeType(date, typeof(DateTime)));
            }
            else if (underlyingType == typeof(string))
            {
                return Expression.Constant(Convert.ToString(value), member.Type);
            }
            else if (underlyingType == typeof(bool))
            {
                return Expression.Constant(Convert.ToBoolean(value), member.Type);
            }
            else
            {
                throw new NotSupportedException($"The nullable underlying type '{underlyingType}' is not supported");
            }
        }
        else
        {
            // Handle non-nullable types
            if (member.Type == typeof(int))
            {
                return Expression.Constant(Convert.ToInt32(value), typeof(int));
            }
            else if (member.Type == typeof(double))
            {
                return Expression.Constant(Convert.ToDouble(value), typeof(double));
            }
            else if (member.Type == typeof(decimal))
            {
                return Expression.Constant(Convert.ToDecimal(value), typeof(decimal));
            }
            else if (member.Type == typeof(DateTime))
            {
                DateTime date;
                if (DateTime.TryParseExact(value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return Expression.Constant(date, typeof(DateTime));
                }
                throw new ArgumentException($"Unable to parse '{value}' as DateTime.");
            }
            else if (member.Type == typeof(string))
            {
                return Expression.Constant(Convert.ToString(value), typeof(string));
            }
            else if (member.Type == typeof(bool))
            {
                return Expression.Constant(Convert.ToBoolean(value), typeof(bool));
            }
            else
            {
                throw new NotSupportedException($"The type '{member.Type}' is not supported");
            }
        }
    }
    private static BinaryExpression GetExpression<T>(
        ParameterExpression param, Filter filter1, Filter filter2)
    {
        Expression bin1 = GetExpression(param, filter1);
        Expression bin2 = GetExpression(param, filter2);

        return Expression.AndAlso(bin1, bin2);
    }
}
