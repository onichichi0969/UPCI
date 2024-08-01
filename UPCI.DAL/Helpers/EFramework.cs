using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace UPCI.DAL.Helpers
{
    public class EFramework
    {
        public static Expression<Func<T, object>> BuildPropertySelector<T>(string propertyNames)
        {
            var parameter = Expression.Parameter(typeof(T), "x");

            // Split the propertyNames string into individual properties
            var propertyNamesArray = propertyNames.Split(',');

            // Start with an empty string expression
            Expression propertyExpression = Expression.Constant(string.Empty, typeof(string));

            foreach (var propertyName in propertyNamesArray)
            {
                var trimmedPropertyName = propertyName.Trim();

                // Get the expression for the current property
                var currentPropertyExpression = GetPropertyExpression<T>(parameter, trimmedPropertyName);

                // Convert the property to its string representation
                var toStringCall = ConvertToStringExpression(currentPropertyExpression);

                // Concatenate with a space using String.Concat
                var space = Expression.Constant(" ", typeof(string));
                propertyExpression = Expression.Call(
                    typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }),
                    propertyExpression,
                    Expression.Condition(
                        Expression.Equal(toStringCall, Expression.Constant(null, typeof(string))),
                        Expression.Constant(string.Empty, typeof(string)),
                        toStringCall
                    )
                );

                // Add a space between concatenated properties (except after the last one)
                if (propertyName != propertyNamesArray.Last())
                {
                    propertyExpression = Expression.Call(
                        typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string), typeof(string) }),
                        propertyExpression,
                        space,
                        Expression.Constant(" ")
                    );
                }
            }

            // Convert the final expression to object
            var convert = Expression.Convert(propertyExpression, typeof(object));
            var lambda = Expression.Lambda<Func<T, object>>(convert, parameter);
            return lambda;
        }

        private static Expression ConvertToStringExpression(Expression propertyExpression)
        {
            var propertyType = propertyExpression.Type;

            // Handle nullable types
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Unwrap the nullable type
                propertyType = Nullable.GetUnderlyingType(propertyType);

                // Handle null check for nullable types
                var hasValueProperty = propertyExpression.Type.GetProperty("HasValue");
                var valueProperty = propertyExpression.Type.GetProperty("Value");
                var nullCheck = Expression.Property(propertyExpression, hasValueProperty);
                var valueExpression = Expression.Property(propertyExpression, valueProperty);

                // Convert the value to string if it has a value
                var toStringMethod = typeof(Convert).GetMethod("ToString", new[] { propertyType });
                var toStringCall = Expression.Call(toStringMethod, valueExpression);

                return Expression.Condition(
                    nullCheck,
                    toStringCall,
                    Expression.Constant(string.Empty, typeof(string))
                );
            }

            // Convert non-nullable property to its string representation
            var nonNullableToStringMethod = typeof(Convert).GetMethod("ToString", new[] { propertyType });
            if (nonNullableToStringMethod == null)
            {
                throw new InvalidOperationException($"Cannot convert property of type '{propertyType.Name}' to string.");
            }

            return Expression.Call(nonNullableToStringMethod, propertyExpression);
        }

        private static Expression GetPropertyExpression<T>(Expression parameter, string propertyName)
        {
            var propertyType = typeof(T);
            var propertyExpression = parameter;

            foreach (var prop in propertyName.Split('.'))
            {
                var propertyInfo = propertyType.GetProperty(prop);
                if (propertyInfo == null)
                {
                    throw new ArgumentException($"Property '{prop}' not found on type '{propertyType.Name}'.");
                }

                propertyExpression = Expression.Property(propertyExpression, propertyInfo);
                propertyType = propertyInfo.PropertyType;
            }

            return propertyExpression;
        }

        public static string GetEntityProperties<T>(T entity) where T : class
        {
            var entityType = entity.GetType();

            var stringBuilder = new StringBuilder();

            foreach (var property in entityType.GetProperties())
            {
                var propertyName = property.Name;

                if (!property.GetCustomAttributes(true).Any(a => a.GetType().Name == "NotMappedAttribute") &&
                    property.GetCustomAttributes(true).All(a => a.GetType().Name != "JsonIgnoreAttribute"))
                {
                    var propertyValue = property.GetValue(entity);

                    stringBuilder.Append($"[{propertyName}: {propertyValue}]");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
