using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Castle.Sharp2Js
{
    /// <summary>
    /// Helper functions for Js Generator
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Gets the name, filtering out the strings provided.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="nameFilters">The name filters.</param>
        /// <returns></returns>
        public static string GetName(string input, List<string> nameFilters)
        {
            return nameFilters == null
                ? input
                : nameFilters.Aggregate(input, (current, nameFilter) => current.Replace(nameFilter, string.Empty));
        }

        /// <summary>
        /// Camel cases an input string.
        /// </summary>
        /// <param name="input">The string.</param>
        /// <param name="camelCase">if set to <c>true</c> [camel case].</param>
        /// <returns></returns>
        public static string ToCamelCase(string input, bool camelCase)
        {
            if (!camelCase) return input;

            var s = input;
            if (!char.IsUpper(s[0])) return s;

            var cArr = s.ToCharArray();
            for (var i = 0; i < cArr.Length; i++)
            {
                if (i > 0 && i + 1 < cArr.Length && !char.IsUpper(cArr[i + 1])) break;
                cArr[i] = char.ToLowerInvariant(cArr[i]);
            }
            return new string(cArr);
        }

        /// <summary>
        /// Determines whether the specified property type is a collection.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        public static bool IsCollectionType(Type propertyType)
        {

            return (propertyType.GetInterfaces().Contains(typeof(IList)) ||
                    propertyType.GetInterfaces().Contains(typeof(ICollection)) ||
                    propertyType.GetInterfaces().Contains(typeof(IDictionary)) ||
                    propertyType.IsArray);
        }

        /// <summary>
        /// Determines whether the specified property type is a dictionary.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        public static bool IsDictionaryType(Type propertyType)
        {
            return (propertyType.GetInterfaces().Contains(typeof(IDictionary)));
        }

        /// <summary>
        /// Determines whether the specified property type is primitive.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        public static bool IsPrimitive(Type propertyType)
        {
            return propertyType.IsPrimitive || propertyType.IsValueType ||
                   propertyType == typeof(string);
        }

        /// <summary>
        /// Determines whether the property should be generated in the Js model.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="generatorOptions">The generator options.</param>
        /// <returns></returns>
        public static bool ShouldGenerateMember(PropertyInfo propertyInfo, JsGeneratorOptions generatorOptions)
        {
            if (!generatorOptions.RespectDataMemberAttribute) return true;

            var customAttributes = propertyInfo.GetCustomAttributes(true);

            return customAttributes.All(p => (p as IgnoreDataMemberAttribute) == null);
        }

        /// <summary>
        /// Determines whether the property has a default value specified by the DefaultValue attribute.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="generatorOptions">The generator options.</param>
        /// <returns></returns>
        public static bool HasDefaultValue(PropertyInfo propertyInfo, JsGeneratorOptions generatorOptions)
        {
            if (!generatorOptions.RespectDefaultValueAttribute) return false;

            var customAttributes = propertyInfo.GetCustomAttributes(true);

            if (customAttributes.All(p => (p as DefaultValueAttribute) == null)) return false;

            return true;
        }

        /// <summary>
        /// Reads the default value from the attribute.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns></returns>
        public static object ReadDefaultValueFromAttribute(PropertyInfo propertyInfo)
        {
            var customAttributes = propertyInfo.GetCustomAttributes(true);

            var defaultValueAttribute = (DefaultValueAttribute)customAttributes.First(p => (p as DefaultValueAttribute) != null);

            return defaultValueAttribute.Value;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="generatorOptions">The generator options.</param>
        /// <returns></returns>
        public static string GetPropertyName(PropertyInfo propertyInfo, JsGeneratorOptions generatorOptions)
        {
            if (!generatorOptions.RespectDataMemberAttribute) return propertyInfo.Name;

            var customAttributes = propertyInfo.GetCustomAttributes(true);

            if (customAttributes.All(p => (p as DataMemberAttribute) == null)) return propertyInfo.Name;

            var dataMemberAttribute = (DataMemberAttribute)customAttributes.First(p => (p as DataMemberAttribute) != null);

            return !string.IsNullOrWhiteSpace(dataMemberAttribute.Name) ? dataMemberAttribute.Name : propertyInfo.Name;
        }
    }
}
