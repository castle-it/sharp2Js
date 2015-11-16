﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Castle.Sharp2Js
{
    /// <summary>
    /// Converts C# classes to javascript objects for use across application tiers and in REST calls, etc.
    /// </summary>
    public static class JsGenerator
    {
        /// <summary>
        /// Global settings for the generator.  These will be used if no override is provided.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public static JsGeneratorOptions Options { get; set; } = new JsGeneratorOptions();

        /// <summary>
        /// Generates a string containing js definitions of the provided types and all implied descendant types.
        /// </summary>
        /// <param name="typesToGenerate">The types to generate.</param>
        /// <param name="generatorOptions">The generator options. Uses global settings if not provided.</param>
        /// <returns></returns>
        public static string Generate(IEnumerable<Type> typesToGenerate, JsGeneratorOptions generatorOptions = null)
        {
            var passedOptions = generatorOptions ?? Options;
            if (passedOptions == null)
            {
                throw new ArgumentNullException(nameof(passedOptions), "Options cannot be null.");
            }
            var propertyClassCollection = GetPropertyDictionaryForTypeGeneration(typesToGenerate, passedOptions);
            var js = GenerateJs(propertyClassCollection, passedOptions);
            return js;
        }


        /// <summary> 
        /// Generates a js equivalent to a C# class and descendant classes. 
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="camelCasePropertyNames">if set to <c>true</c>, use camel casing in the output model.</param>
        /// <param name="outputNamespace">The output namespace.</param>
        /// <returns>A javsacript object string.</returns>
        [Obsolete("This is a legacy method. Please use the Generate(...) method instead.")]
        public static string GenerateJsModelFromTypeWithDescendants(Type modelType, bool camelCasePropertyNames, string outputNamespace)
        {
            var propertyDictionary = GetPropertyDictionaryForTypeGeneration(new[] { modelType }, Options);

            return GenerateJs(propertyDictionary, new JsGeneratorOptions()
            {
                CamelCase = camelCasePropertyNames,
                ClassNameConstantsToRemove = new List<string>() { "Dto" },
                OutputNamespace = outputNamespace,
                IncludeMergeFunction = true
            });

        }

        /// <summary>
        /// Generates the js.
        /// </summary>
        /// <param name="propertyCollection">The property collection derived from the types to be converted.</param>
        /// <param name="generationOptions">The generation options.</param>
        /// <returns></returns>
        private static string GenerateJs(IEnumerable<PropertyBag> propertyCollection, JsGeneratorOptions generationOptions)
        {
            var options = generationOptions;
            
            var sbOut = new StringBuilder();

            foreach (var type in propertyCollection.GroupBy(r => r.TypeName))
            {
                var sb = new StringBuilder();

                if (type.Any(p => !p.IsPrimitiveType))
                {
                    sb.AppendLine(
                        $"{options.OutputNamespace}.{GetName(type.First().TypeName, options.ClassNameConstantsToRemove)} = function (cons, overrideObj) {{");
                    sb.AppendLine("\tif (!overrideObj) { overrideObj = { }; }");
                }
                else
                {
                    sb.AppendLine(
                        $"{options.OutputNamespace}.{GetName(type.First().TypeName, options.ClassNameConstantsToRemove)} = function (cons) {{");
                }

                sb.AppendLine("\tif (!cons) { cons = { }; }");

                if (type.Any(p => p.IsArray))
                {
                    sb.AppendLine("\tvar i, length;");
                }

                sb.AppendLine();

                var propList = type.GroupBy(t => t.PropertyName).Select(t => t.First()).ToList();
                foreach (var propEntry in propList)
                {
                    if (propEntry.IsArray)
                    {
                        sb.AppendLine(string.Format("\tthis.{0} = new Array(cons.{0} == null ? 0 : cons.{1}.length );",
                            ToCamelCase(propEntry.PropertyName, options.CamelCase),
                            ToCamelCase(propEntry.PropertyName, options.CamelCase)));
                        sb.AppendLine($"\tif(cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} != null) {{");
                        sb.AppendLine(
                            $"\t\tfor (i = 0, length = cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}.length; i < length; i++) {{");

                        if (!propEntry.IsPrimitiveType)
                        {
                            sb.AppendLine(
                                $"\t\t\tif (!overrideObj.{ GetName(propEntry.PropertyTypeName, options.ClassNameConstantsToRemove) }) {{");
                            sb.AppendLine(
                                $"\t\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i] = new {options.OutputNamespace}.{ GetName(propEntry.PropertyTypeName, options.ClassNameConstantsToRemove) }(cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i]);");
                            sb.AppendLine("\t\t\t} else {");
                            sb.AppendLine(
                                $"\t\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i] = new overrideObj.{ GetName(propEntry.PropertyTypeName, options.ClassNameConstantsToRemove) }(cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i], overrideObj);");

                            sb.AppendLine("\t\t\t}");
                        }
                        else
                        {
                            sb.AppendLine(
                                $"\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i] = cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i];");
                        }
                        sb.AppendLine("\t\t}");
                        sb.AppendLine("\t}");
                    }
                    else if (!propEntry.IsPrimitiveType)
                    {
                        sb.AppendLine(
                            $"\tif (!overrideObj.{ GetName(propEntry.PropertyTypeName, options.ClassNameConstantsToRemove) }) {{");
                        sb.AppendLine(
                            $"\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = new {options.OutputNamespace}.{ GetName(propEntry.PropertyTypeName, options.ClassNameConstantsToRemove) }(cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)});");
                        sb.AppendLine("\t} else {");
                        sb.AppendLine(
                            $"\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = new overrideObj.{ GetName(propEntry.PropertyTypeName, options.ClassNameConstantsToRemove) }(cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}, overrideObj);");

                        sb.AppendLine("\t}");
                    }
                    else
                    {
                        if (propEntry.HasDefaultValue)
                        {
                            sb.AppendLine(
                            $"\tif (!cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                            sb.AppendLine(
                                propEntry.PropertyType == typeof (string)
                                    ? $"\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = '{propEntry.DefaultValue}';"
                                    : $"\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = {propEntry.DefaultValue};");
                            sb.AppendLine("\t} else {");
                            sb.AppendLine(
                            $"\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)};");
                            sb.AppendLine("\t}");
                            
                        }
                        else
                        {
                            sb.AppendLine(
                            $"\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)};");
                        }
                        
                    }
                }

                if (options.IncludeMergeFunction)
                {
                    //Generate a merge function to merge two objects
                    sb.AppendLine();
                    sb.AppendLine("\tthis.$merge = function (mergeObj) {");
                    sb.AppendLine("\t\tif (!mergeObj) { mergeObj = { }; }");
                    foreach (var propEntry in propList)
                    {
                        if (propEntry.IsArray)
                        {
                            sb.AppendLine(
                                $"\t\tif (!mergeObj.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                            sb.AppendLine($"\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = null;");
                            sb.AppendLine("\t\t}");
                            sb.AppendLine(
                                $"\t\tif (this.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} != null) {{");
                            sb.AppendLine(string.Format("\t\t\tthis.{0}.splice(0, this.{0}.length);",
                                ToCamelCase(propEntry.PropertyName, options.CamelCase)));
                            sb.AppendLine("\t\t}");
                            sb.AppendLine(
                                $"\t\tif (mergeObj.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                            sb.AppendLine(
                                $"\t\t\tif (this.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} === null) {{");
                            sb.AppendLine($"\t\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = [];");
                            sb.AppendLine("\t\t\t}");
                            sb.AppendLine(
                                $"\t\t\tfor (i = 0; i < mergeObj.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}.length; i++) {{");
                            sb.AppendLine(string.Format("\t\t\t\tthis.{0}.push(mergeObj.{0}[i]);",
                                ToCamelCase(propEntry.PropertyName, options.CamelCase)));
                            sb.AppendLine("\t\t\t}");
                            sb.AppendLine("\t\t}");

                        }
                        else if (!propEntry.IsPrimitiveType)
                        {
                            sb.AppendLine(
                                $"\t\tif (mergeObj.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} == null) {{");
                            sb.AppendLine($"\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = null;");
                            sb.AppendLine(
                                $"\t\t}} else if (this.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} != null) {{");
                            sb.AppendLine(
                                $"\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}.$merge(mergeObj.{ToCamelCase(propEntry.PropertyName, options.CamelCase)});");
                            sb.AppendLine("\t\t} else {");
                            sb.AppendLine(
                                $"\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = mergeObj.{ToCamelCase(propEntry.PropertyName, options.CamelCase)};");
                            sb.AppendLine("\t\t}");
                        }
                        else
                        {
                            sb.AppendLine(
                                $"\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = mergeObj.{ToCamelCase(propEntry.PropertyName, options.CamelCase)};");
                        }

                    }
                    sb.AppendLine("\t}");
                }

                sb.AppendLine("}");
                sbOut.AppendLine(sb.ToString());
                sbOut.AppendLine();
            }

            return sbOut.ToString();
        }

        /// <summary>
        /// Gets the name, filtering out the strings provided.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="nameFilters">The name filters.</param>
        /// <returns></returns>
        private static string GetName(string input, List<string> nameFilters)
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
        private static string ToCamelCase(string input, bool camelCase)
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
        /// Gets the property dictionary to be used for type generation.
        /// </summary>
        /// <param name="types">The types to generate property information for.</param>
        /// <param name="generatorOptions">The generator options.</param>
        /// <param name="propertyTypeCollection">The output collection of properties discovered through reflection of the supplied classes.</param>
        /// <returns></returns>
        private static IEnumerable<PropertyBag> GetPropertyDictionaryForTypeGeneration(IEnumerable<Type> types,
            JsGeneratorOptions generatorOptions,
            List<PropertyBag> propertyTypeCollection = null)
        {
            if (propertyTypeCollection == null)
            {
                propertyTypeCollection = new List<PropertyBag>();
            }
            foreach (var type in types)
            {

                var props = type.GetProperties();
                var typeName = type.Name;
                foreach (var prop in props)
                {
                    if (ShouldGenerateMember(prop, generatorOptions))
                    {
                        var propertyName = GetPropertyName(prop, generatorOptions);
                        var propertyType = prop.PropertyType;

                        if (!propertyType.IsPrimitive && !propertyType.IsValueType &&
                            propertyType != typeof (string))
                        {

                            if ((propertyType.IsGenericType &&
                                 propertyType.GetGenericTypeDefinition() == typeof (List<>)) || propertyType.IsArray)
                            {
                                var elementType = propertyType.IsArray
                                    ? propertyType.GetElementType()
                                    : propertyType.GetGenericArguments()[0];
                                var isPrimitive = elementType.IsPrimitive || elementType.IsValueType ||
                                                  (elementType == typeof (string));

                                propertyTypeCollection.Add(new PropertyBag(typeName, propertyName, propertyType, true,
                                    elementType.Name, isPrimitive, false, null));

                                if (!isPrimitive)
                                {
                                    if (propertyTypeCollection.All(p => p.TypeName != elementType.Name))
                                    {
                                        propertyTypeCollection.AddRange(
                                            GetPropertyDictionaryForTypeGeneration(new[] {elementType},
                                                generatorOptions, propertyTypeCollection));
                                    }
                                }
                            }
                            else
                            {
                                propertyTypeCollection.Add(new PropertyBag(typeName, propertyName, propertyType, false,
                                    propertyType.Name, false, false, null));
                                if (propertyTypeCollection.All(p => p.TypeName != propertyType.Name))
                                {
                                    propertyTypeCollection.AddRange(
                                        GetPropertyDictionaryForTypeGeneration(new[] {propertyType},
                                            generatorOptions, propertyTypeCollection));
                                }
                            }
                        }
                        else
                        {
                            var hasDefaultValue = HasDefaultValue(prop, generatorOptions);
                            if (hasDefaultValue)
                            {
                                var val = ReadDefaultValueFromAttribute(prop, generatorOptions);
                                propertyTypeCollection.Add(new PropertyBag(typeName, propertyName, propertyType, false,
                                string.Empty, true, true, val));
                            }
                            else
                            {
                                propertyTypeCollection.Add(new PropertyBag(typeName, propertyName, propertyType, false,
                                string.Empty, true, false, null));
                            }
                            
                        }
                    }
                }
            }
            return propertyTypeCollection;
        }

        private static bool ShouldGenerateMember(PropertyInfo propertyInfo, JsGeneratorOptions generatorOptions)
        {
            if (!generatorOptions.RespectDataMemberAttribute) return true;

            var customAttributes = propertyInfo.GetCustomAttributes(true);

            return customAttributes.All(p => (p as IgnoreDataMemberAttribute) == null);
        }

        private static bool HasDefaultValue(PropertyInfo propertyInfo, JsGeneratorOptions generatorOptions)
        {
            if (!generatorOptions.RespectDefaultValueAttribute) return false;

            var customAttributes = propertyInfo.GetCustomAttributes(true);

            if (customAttributes.All(p => (p as DefaultValueAttribute) == null)) return false;

            return true;
        }
        private static object ReadDefaultValueFromAttribute(PropertyInfo propertyInfo, JsGeneratorOptions generatorOptions)
        {
            var customAttributes = propertyInfo.GetCustomAttributes(true);

            var defaultValueAttribute = (DefaultValueAttribute)customAttributes.First(p => (p as DefaultValueAttribute) != null);

            return defaultValueAttribute.Value;
        }

        private static string GetPropertyName(PropertyInfo propertyInfo, JsGeneratorOptions generatorOptions)
        {
            if (!generatorOptions.RespectDataMemberAttribute) return propertyInfo.Name;

            var customAttributes = propertyInfo.GetCustomAttributes(true);

            if (customAttributes.All(p => (p as DataMemberAttribute) == null)) return propertyInfo.Name;

            var dataMemberAttribute = (DataMemberAttribute)customAttributes.First(p => (p as DataMemberAttribute) != null);

            return !string.IsNullOrWhiteSpace(dataMemberAttribute.Name) ? dataMemberAttribute.Name : propertyInfo.Name;
        }
    }
}
