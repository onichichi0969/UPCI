using UPCI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPCI.DAL.Helpers
{
    public class SQLWhereClauseParser
    {
        public static List<Filter> Parse(string sqlWhereClause)
        {
            var filters = new List<Filter>();

            // Example parsing logic for a simplified SQL WHERE clause
            var conditions = sqlWhereClause.Split(new[] { "AND", "OR" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var condition in conditions)
            {
                if (condition.Contains("LIKE"))
                {
                    filters.Add(new Filter
                    {
                        Property = ExtractProperty(condition, "LIKE"),
                        Operator = Operators.Contains,
                        //Value = ExtractValue(condition, "LIKE")
                    });
                }
                else if (condition.Contains(">="))
                {
                    filters.Add(new Filter
                    {
                        Property = ExtractProperty(condition, ">="),
                        Operator = Operators.GreaterThanOrEqual,
                        //Value = ExtractValue(condition, ">=")
                    });
                }
                else if (condition.Contains("<="))
                {
                    filters.Add(new Filter
                    {
                        Property = ExtractProperty(condition, "<="),
                        Operator = Operators.LessThanOrEqual,
                        //Value = ExtractValue(condition, "<=")
                    });
                }
                else if (condition.Contains(">"))
                {
                    filters.Add(new Filter
                    {
                        Property = ExtractProperty(condition, ">"),
                        Operator = Operators.GreaterThan,
                        //Value = ExtractValue(condition, ">")
                    });
                }
                else if (condition.Contains("<"))
                {
                    filters.Add(new Filter
                    {
                        Property = ExtractProperty(condition, "<"),
                        Operator = Operators.LessThan,
                        //Value = ExtractValue(condition, "<")
                    });
                }
                else if (condition.Contains("="))
                {
                    filters.Add(new Filter
                    {
                        Property = ExtractProperty(condition, "="),
                        Operator = Operators.Equals,
                        //Value = ExtractValue(condition, "=")
                    });
                }
                else if (condition.Contains("<>"))
                {
                    filters.Add(new Filter
                    {
                        Property = ExtractProperty(condition, "<>"),
                        Operator = Operators.NotEquals,
                        //Value = ExtractValue(condition, "<>")
                    });
                }
                else if (condition.Contains("BETWEEN"))
                {
                    var values = condition.Split(new[] { "BETWEEN", "AND" }, StringSplitOptions.RemoveEmptyEntries);
                    filters.Add(new Filter
                    {
                        Property = ExtractProperty(condition, "BETWEEN"),
                        Operator = Operators.Between,
                        Value = values[1].Trim(),
                        Value2 = values[2].Trim()
                    });
                }
                else if (condition.Contains("IN"))
                {
                    var property = ExtractProperty(condition, "IN");
                    var values = ExtractInValues(condition);
                    filters.Add(new Filter
                    {
                        Property = property,
                        Operator = Operators.In,
                        Values = values
                    });
                }
                else if (condition.Contains("IS NULL"))
                {
                    filters.Add(new Filter
                    {
                        Property = ExtractProperty(condition, "IS NULL"),
                        Operator = Operators.IsNull
                    });
                }
                else if (condition.Contains("IS NOT NULL"))
                {
                    filters.Add(new Filter
                    {
                        Property = ExtractProperty(condition, "IS NOT NULL"),
                        Operator = Operators.IsNotNull
                    });
                }
            }

            return filters;
        }
        private static string ExtractProperty(string condition, string op)
        {
            return condition.Split(new[] { op }, StringSplitOptions.None)[0].Trim();
        }

        private static object ExtractValue(string condition, string op)
        {
            return condition.Split(new[] { op }, StringSplitOptions.None)[1].Trim().Trim('\'');
        }

        private static IEnumerable<object> ExtractInValues(string condition)
        {
            var startIndex = condition.IndexOf('(') + 1;
            var endIndex = condition.IndexOf(')');
            var valuesString = condition.Substring(startIndex, endIndex - startIndex);
            var values = valuesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(v => v.Trim().Trim('\''));
            return values;
        }
    }
}
