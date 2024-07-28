using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace UPCI.DAL.Helpers
{
    public class EFramework
    {
        public static Expression<Func<T, object>> BuildPropertySelector<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression propertyExpression = parameter;
            Type propertyType = typeof(T);

            foreach (var prop in propertyName.Split('.'))
            {
                PropertyInfo propertyInfo = propertyType.GetProperty(prop);
                if (propertyInfo == null)
                {
                    throw new ArgumentException($"Property '{prop}' not found on type '{propertyType.Name}'.");
                }

                propertyExpression = Expression.Property(propertyExpression, propertyInfo);
                propertyType = propertyInfo.PropertyType;
            }

            var convert = Expression.Convert(propertyExpression, typeof(object));
            var lambda = Expression.Lambda<Func<T, object>>(convert, parameter);
            return lambda;
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
                    var propertyValue = entity.GetType().GetProperty(propertyName)?.GetValue(entity);

                    stringBuilder.Append($"[{propertyName}: {propertyValue}]");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
