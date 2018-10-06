using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Aquiva.Connector.ScribeApi.Metadata
{

    public static class ReflectionExtensions
    {
        public static bool IsNullable(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsEnumerable(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
        }

        public static bool IsFromMscorlib(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.Assembly == typeof(int).Assembly;
        }

        public static Type GetPropertyDefinitionType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.IsArray
                ? type.GetElementType().IsNullable()
                    ? type.GetElementType()?.GenericTypeArguments.First()
                    : type.GetElementType()
                : type.IsNullable()
                    ? type.GenericTypeArguments.First()
                    : type.IsEnumerable()
                        ? type.GenericTypeArguments.Any()
                            ? type.GenericTypeArguments.First().IsNullable()
                                ? type.GenericTypeArguments.First().GenericTypeArguments.First()
                                : type.GenericTypeArguments.First()
                            : typeof(object)
                        : type;
        }

        public static string GetTypeName(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return IsNullable(type)
                ? type.GenericTypeArguments.First().FullName
                : type.FullName;
        }

        public static string GetObjectDefinitionFullName<T>() => typeof(T).GetObjectDefinitionFullName();

        public static string GetObjectDefinitionFullName(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var definitions = type.GetCustomAttributes<ObjectDefinitionAttribute>().ToList();

            if (!definitions.Any())
            {
                throw new ArgumentException(
                    $"Can't find object definition for {type.FullName} type",
                    nameof(type));
            }
            
            return string.IsNullOrWhiteSpace(definitions.Single().FullName)
                ? type.Name
                : definitions.Single().FullName.Trim();
        }

        public static string GetPropertyDefinitionFullName<T>(string propertyName) => 
            typeof(T).GetPropertyDefinitionFullName(propertyName);

        public static string GetPropertyDefinitionFullName(this Type type, string propertyName)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var definitions = type.GetMembers()
                .Where(m => m.Name == propertyName)
                .SelectMany(m => m.GetCustomAttributes<PropertyDefinitionAttribute>())
                .ToList();

            if (!definitions.Any())
            {
                throw new ArgumentException(
                    $"Can't find property definition for {propertyName} property in {type.FullName} type",
                    nameof(propertyName));
            }
            
            return string.IsNullOrWhiteSpace(definitions.Single().FullName)
                ? propertyName
                : definitions.Single().FullName.Trim();
        }
    }
}
