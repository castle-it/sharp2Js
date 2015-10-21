using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Castle.Sharp2Js
{
    public static class JsGenerator
    {
        /// <summary> 
        /// Generates a js equivalent to a C# class and descendant classes.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="camelCasePropertyNames">if set to <c>true</c>, use camel casing in the output model.</param>
        /// <param name="outputNamespace">The output namespace.</param>
        /// <returns>A javsacript object string.</returns>
        public static string GenerateJsModelFromTypeWithDescendants(Type modelType, bool camelCasePropertyNames, string outputNamespace)
        {
            var propertyDictionary = GetPropertyDictionaryForTypeGeneration(modelType);

            var sbOut = new StringBuilder();

            foreach (var type in propertyDictionary.GroupBy(r => r.TypeName))
            {
                var sb = new StringBuilder();
                sb.AppendLine(
                    $"{outputNamespace}.{type.First().TypeName.Replace("Dto", string.Empty)} = function (cons, overrideObj) {{");
                sb.AppendLine("\tif (!overrideObj) { overrideObj = { }; }");
                sb.AppendLine("\tif (!cons) { cons = { }; }");
                sb.AppendLine("\tvar i, length;");

                var propList = type.GroupBy(t => t.PropertyName).Select(t => t.First()).ToList();
                foreach (var propEntry in propList)
                {
                    if (propEntry.IsGenericList)
                    {
                        sb.AppendLine(string.Format("\tthis.{0} = new Array(cons.{0} == null ? 0 : cons.{1}.length );",
                            ToCamelCase(propEntry.PropertyName, camelCasePropertyNames),
                            ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)));
                        sb.AppendLine($"\tif(cons.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)} != null) {{");
                        sb.AppendLine(
                            $"\t\tfor (i = 0, length = cons.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)}.length; i < length; i++) {{");

                        if (!propEntry.IsPrimitiveType)
                        {

                            sb.AppendLine(
                                $"\t\t\tif (!overrideObj.{propEntry.PropertyTypeName.Replace("Dto", string.Empty)}) {{");
                            sb.AppendLine(
                                $"\t\t\t\tthis.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)}[i] = new {outputNamespace}.{propEntry.PropertyTypeName.Replace("Dto", string.Empty)}(cons.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)}[i]);");
                            sb.AppendLine("\t\t\t} else {");
                            sb.AppendLine(
                                $"\t\t\t\tthis.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)}[i] = new overrideObj.{propEntry.PropertyTypeName.Replace("Dto", string.Empty)}(cons.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)}[i], overrideObj);");

                            sb.AppendLine("\t\t\t}");
                        }
                        else
                        {
                            sb.AppendLine(
                                $"\t\t\tthis.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)}[i] = cons.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)}[i];");
                        }
                        sb.AppendLine("\t\t}");
                        sb.AppendLine("\t}");
                    }
                    else if (!propEntry.IsPrimitiveType)
                    {
                        sb.AppendLine(
                            $"\tif (!overrideObj.{propEntry.PropertyTypeName.Replace("Dto", string.Empty)}) {{");
                        sb.AppendLine(
                            $"\t\tthis.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)} = new {outputNamespace}.{propEntry.PropertyTypeName.Replace("Dto", string.Empty)}(cons.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)});");
                        sb.AppendLine("\t} else {");
                        sb.AppendLine(
                            $"\t\tthis.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)} = new overrideObj.{propEntry.PropertyTypeName.Replace("Dto", string.Empty)}(cons.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)}, overrideObj);");

                        sb.AppendLine("\t}");
                    }
                    else
                    {
                        sb.AppendLine(
                            $"\tthis.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)} = cons.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)};");
                    }


                }
                sb.AppendLine();
                sb.AppendLine();

                //Generate a merge function to merge two objects

                sb.AppendLine("\tthis.$merge = function (mergeObj) {");
                sb.AppendLine("\t\tif (!mergeObj) { mergeObj = { }; }");
                foreach (var propEntry in propList)
                {
                    if (propEntry.IsGenericList)
                    {
                        sb.AppendLine($"\t\tif (!mergeObj.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)}) {{");
                        sb.AppendLine($"\t\t\tthis.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)} = null;");
                        sb.AppendLine("\t\t}");
                        sb.AppendLine($"\t\tif (this.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)} != null) {{");
                        sb.AppendLine(string.Format("\t\t\tthis.{0}.splice(0, this.{0}.length);",
                            ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)));
                        sb.AppendLine("\t\t}");
                        sb.AppendLine($"\t\tif (mergeObj.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)}) {{");
                        sb.AppendLine($"\t\t\tif (this.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)} === null) {{");
                        sb.AppendLine($"\t\t\t\tthis.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)} = [];");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine(
                            $"\t\t\tfor (i = 0; i < mergeObj.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)}.length; i++) {{");
                        sb.AppendLine(string.Format("\t\t\t\tthis.{0}.push(mergeObj.{0}[i]);", ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)));
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine("\t\t}");

                    }
                    else if (!propEntry.IsPrimitiveType)
                    {
                        sb.AppendLine($"\t\tif (mergeObj.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)} == null) {{");
                        sb.AppendLine($"\t\t\tthis.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)} = null;");
                        sb.AppendLine($"\t\t}} else if (this.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)} != null) {{");
                        sb.AppendLine(
                            $"\t\t\tthis.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)}.$merge(mergeObj.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)});");
                        sb.AppendLine("\t\t} else {");
                        sb.AppendLine(
                            $"\t\t\tthis.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)} = mergeObj.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)};");
                        sb.AppendLine("\t\t}");
                    }
                    else
                    {
                        sb.AppendLine(
                            $"\t\tthis.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)} = mergeObj.{ToCamelCase(propEntry.PropertyName, camelCasePropertyNames)};");
                    }

                }
                sb.AppendLine("\t}");


                sb.AppendLine("}");
                sbOut.AppendLine(sb.ToString());
                sbOut.AppendLine();
            }

            return sbOut.ToString();

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

            return input.Substring(0, 1).ToLower() +
                input.Substring(1);
        }
        /// <summary>
        /// Gets the property dictionary to be used for type generation.
        /// </summary>
        /// <param name="type">The type to generate property information for.</param>
        /// <returns></returns>
        private static IEnumerable<PropertyBag> GetPropertyDictionaryForTypeGeneration(Type type)
        {

            var outList = new List<PropertyBag>();
            var props = type.GetProperties();
            var tName = type.Name;
            foreach (var prop in props)
            {

                if (!prop.PropertyType.IsPrimitive && !prop.PropertyType.IsValueType &&
                    prop.PropertyType != typeof(string))
                {
                    //maybe has kids
                    if (prop.PropertyType.IsGenericType &&
                        prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {

                        Type itemType = prop.PropertyType.GetGenericArguments()[0];
                        var isPrim = itemType.IsPrimitive || itemType.IsValueType || (itemType == typeof(string));
                        outList.Add(new PropertyBag(tName, prop.Name, prop.PropertyType, true, itemType.Name, isPrim));
                        if (!isPrim)
                        {
                            outList.AddRange(GetPropertyDictionaryForTypeGeneration(itemType));
                        }

                    }
                    else
                    {
                        outList.Add(new PropertyBag(tName, prop.Name, prop.PropertyType, false, prop.PropertyType.Name, false));
                        outList.AddRange(GetPropertyDictionaryForTypeGeneration(prop.PropertyType));
                    }
                }
                else
                {
                    outList.Add(new PropertyBag(tName, prop.Name, prop.PropertyType, false, string.Empty, true));
                }
            }
            return outList;
        }
    }
}
