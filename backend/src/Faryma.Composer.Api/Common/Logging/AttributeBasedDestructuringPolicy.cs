using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Serilog.Core;
using Serilog.Events;

namespace Faryma.Composer.Api.Common.Logging
{
    public sealed class AttributeBasedDestructuringPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
        {
            result = null;

            Type type = value.GetType();

            if (!ShouldHandle(type))
            {
                return false;
            }

            List<LogEventProperty> properties = new();

            foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!propertyInfo.CanRead)
                {
                    continue;
                }

                if (propertyInfo.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                if (propertyInfo.GetCustomAttribute<LogIgnoreAttribute>() is not null)
                {
                    continue;
                }

                object? propertyValue;
                try
                {
                    propertyValue = propertyInfo.GetValue(value);
                }
                catch
                {
                    continue;
                }

                properties.Add(new LogEventProperty(
                    propertyInfo.Name,
                    propertyValueFactory.CreatePropertyValue(propertyValue, destructureObjects: true)));
            }

            result = new StructureValue(properties);

            return true;
        }

        private static bool ShouldHandle(Type type)
        {
            if (type == typeof(string))
            {
                return false;
            }

            if (type.IsPrimitive || type.IsEnum)
            {
                return false;
            }

            if (type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(DateOnly)
                || type == typeof(TimeOnly)
                || type == typeof(TimeSpan)
                || type == typeof(Guid))
            {
                return false;
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return false;
            }

            return true;
        }
    }
}
