using System;
using System.Collections;
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

        private static readonly List<Type> AllowedDictionaryKeyTypes = new List<Type>()
        {
            typeof(int),
            typeof(string),
            typeof(Enum)
        }; 

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
                var typeDefinition = type.First().TypeDefinition;

                var sb = new StringBuilder();

                BuildClassConstructor(type, sb, options);

                //initialize array variables if any are present in this type
                if (
                    type.Any(
                        p =>
                            p.TransformablePropertyType == PropertyBag.TransformablePropertyTypeEnum.CollectionType ||
                            p.TransformablePropertyType == PropertyBag.TransformablePropertyTypeEnum.DictionaryType))
                {
                    sb.AppendLine("\tvar i, length;");
                }

                sb.AppendLine();

                var propList = type.GroupBy(t => t.PropertyName).Select(t => t.First()).ToList();
                foreach (var propEntry in propList)
                {
                    switch (propEntry.TransformablePropertyType)
                    {
                        case PropertyBag.TransformablePropertyTypeEnum.CollectionType:
                            BuildArrayProperty(sb, propEntry, options);
                            break;
                        case PropertyBag.TransformablePropertyTypeEnum.DictionaryType:
                            BuildDictionaryProperty(sb, propEntry, options);
                            break;
                        case PropertyBag.TransformablePropertyTypeEnum.ReferenceType:
                            BuildObjectProperty(sb, propEntry, options);
                            break;
                        case PropertyBag.TransformablePropertyTypeEnum.Primitive:
                            BuildPrimitiveProperty(propEntry, sb, options);
                            break;
                    }
                }

                if (options.IncludeMergeFunction && !typeDefinition.IsEnum)
                {
                    sb.AppendLine();
                    BuildMergeFunctionForClass(sb, propList, options);
                }

                if (options.CustomFunctionProcessors?.Any() == true)
                {
                    foreach (var customProcessor in options.CustomFunctionProcessors)
                    {
                        sb.AppendLine();
                        customProcessor(sb, propList, options);
                    }
                }

                BuildClassClosure(sb);

                sbOut.AppendLine(sb.ToString());
                sbOut.AppendLine();
            }

            return sbOut.ToString();
        }

        /// <summary>
        /// Builds the Js class closure.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        private static void BuildClassClosure(StringBuilder sb)
        {
            sb.AppendLine("}");
        }

        /// <summary>
        /// Builds the class constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="sb">The string builder.</param>
        /// <param name="options">The options.</param>
        private static void BuildClassConstructor(IGrouping<string, PropertyBag> type, StringBuilder sb, JsGeneratorOptions options)
        {
            if (
                type.Any(
                    p =>
                        (p.CollectionInnerTypes != null && p.CollectionInnerTypes.Any(q => !q.IsPrimitiveType)) ||
                        p.TransformablePropertyType == PropertyBag.TransformablePropertyTypeEnum.ReferenceType))
            {
                sb.AppendLine(
                    $"{options.OutputNamespace}.{GetName(type.First().TypeName, options.ClassNameConstantsToRemove)} = function (cons, overrideObj) {{");
                sb.AppendLine("\tif (!overrideObj) { overrideObj = { }; }");
                sb.AppendLine("\tif (!cons) { cons = { }; }");
            }
            else if (type.First().TypeDefinition.IsEnum)
            {
                sb.AppendLine(
                    $"{options.OutputNamespace}.{GetName(type.First().TypeName, options.ClassNameConstantsToRemove)} = {{");
            }
            else
            {
                sb.AppendLine(
                    $"{options.OutputNamespace}.{GetName(type.First().TypeName, options.ClassNameConstantsToRemove)} = function (cons) {{");

                sb.AppendLine("\tif (!cons) { cons = { }; }");
            }
            
        }

        /// <summary>
        /// Builds the merge function for a type.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="propList">The property list.</param>
        /// <param name="options">The options.</param>
        private static void BuildMergeFunctionForClass(StringBuilder sb, IEnumerable<PropertyBag> propList,
                    JsGeneratorOptions options)
        {
            //Generate a merge function to merge two objects
            sb.AppendLine("\tthis.$merge = function (mergeObj) {");
            sb.AppendLine("\t\tif (!mergeObj) { mergeObj = { }; }");
            foreach (var propEntry in propList)
            {
                switch (propEntry.TransformablePropertyType)
                {
                    case PropertyBag.TransformablePropertyTypeEnum.CollectionType:
                        sb.AppendLine(
                            $"\t\tif (!mergeObj.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine($"\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = [];");
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
                        break;
                    case PropertyBag.TransformablePropertyTypeEnum.DictionaryType:
                        sb.AppendLine(
                            $"\t\tif (this.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} != null) {{");
                        sb.AppendLine(
                            $"\t\t\tfor (var key in this.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine(
                            $"\t\t\t\tif (this.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}.hasOwnProperty(key)) {{");
                        sb.AppendLine(
                            $"\t\t\t\t\tdelete this.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key];");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine("\t\t}");
                        sb.AppendLine(
                            $"\t\tif (mergeObj.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine(
                            $"\t\t\tfor (var key in mergeObj.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine(
                            $"\t\t\t\tif (mergeObj.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}.hasOwnProperty(key)) {{");
                        sb.AppendLine(
                            $"\t\t\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key] = mergeObj.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key];");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine("\t\t}");
                        break;
                    case PropertyBag.TransformablePropertyTypeEnum.ReferenceType:
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
                        break;
                    case PropertyBag.TransformablePropertyTypeEnum.Primitive:
                        sb.AppendLine(
                            $"\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = mergeObj.{ToCamelCase(propEntry.PropertyName, options.CamelCase)};");
                        break;
                }
            }
            sb.AppendLine("\t}");
        }

        /// <summary>
        /// Builds a primitive property.
        /// </summary>
        /// <param name="propEntry">The property entry.</param>
        /// <param name="sb">The string builder.</param>
        /// <param name="options">The options.</param>
        private static void BuildPrimitiveProperty(PropertyBag propEntry, StringBuilder sb, JsGeneratorOptions options)
        {
            if (propEntry.TypeDefinition.IsEnum)
            {
                sb.AppendLine(
                    propEntry.PropertyType == typeof(string)
                        ? $"\t{ToCamelCase(propEntry.PropertyName, options.CamelCase)}: '{propEntry.DefaultValue}',"
                        : $"\t{ToCamelCase(propEntry.PropertyName, options.CamelCase)}: {propEntry.DefaultValue},");
            }
            else if (propEntry.HasDefaultValue)
            {
                sb.AppendLine(
                    $"\tif (!cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                sb.AppendLine(
                    propEntry.PropertyType == typeof(string)
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

        /// <summary>
        /// Builds an object/reference property.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="propEntry">The property entry.</param>
        /// <param name="options">The options.</param>
        private static void BuildObjectProperty(StringBuilder sb, PropertyBag propEntry, JsGeneratorOptions options)
        {

            sb.AppendLine($"\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = null;");
            sb.AppendLine($"\tif (cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
            sb.AppendLine(
                $"\t\tif (!overrideObj.{GetName(propEntry.PropertyType.Name, options.ClassNameConstantsToRemove)}) {{");
            sb.AppendLine(
                $"\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = new {options.OutputNamespace}.{GetName(propEntry.PropertyType.Name, options.ClassNameConstantsToRemove)}(cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)});");
            sb.AppendLine("\t\t} else {");
            sb.AppendLine(
                $"\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = new overrideObj.{GetName(propEntry.PropertyType.Name, options.ClassNameConstantsToRemove)}(cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}, overrideObj);");

            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
        }

        /// <summary>
        /// Builds an array property.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="propEntry">The property entry.</param>
        /// <param name="options">The options.</param>
        private static void BuildArrayProperty(StringBuilder sb, PropertyBag propEntry, JsGeneratorOptions options)
        {
            sb.AppendLine(string.Format("\tthis.{0} = new Array(cons.{0} == null ? 0 : cons.{1}.length );",
                ToCamelCase(propEntry.PropertyName, options.CamelCase),
                ToCamelCase(propEntry.PropertyName, options.CamelCase)));
            sb.AppendLine($"\tif(cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} != null) {{");
            sb.AppendLine(
                $"\t\tfor (i = 0, length = cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}.length; i < length; i++) {{");

            var collectionType = propEntry.CollectionInnerTypes.First();

            if (!collectionType.IsPrimitiveType)
            {
                sb.AppendLine(
                    $"\t\t\tif (!overrideObj.{GetName(collectionType.Type.Name, options.ClassNameConstantsToRemove)}) {{");
                sb.AppendLine(
                    $"\t\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i] = new {options.OutputNamespace}.{GetName(collectionType.Type.Name, options.ClassNameConstantsToRemove)}(cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i]);");
                sb.AppendLine("\t\t\t} else {");
                sb.AppendLine(
                    $"\t\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i] = new overrideObj.{GetName(collectionType.Type.Name, options.ClassNameConstantsToRemove)}(cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i], overrideObj);");

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

        /// <summary>
        /// Builds a dictionary property.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="propEntry">The property entry.</param>
        /// <param name="options">The options.</param>
        private static void BuildDictionaryProperty(StringBuilder sb, PropertyBag propEntry, JsGeneratorOptions options)
        {
            sb.AppendLine($"\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} = {{}};");
            sb.AppendLine($"\tif(cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)} != null) {{");
            sb.AppendLine(
                $"\t\tfor (var key in cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
            sb.AppendLine(
                $"\t\t\tif (cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}.hasOwnProperty(key)) {{");

            var keyType = propEntry.CollectionInnerTypes.First(p => p.IsDictionaryKey);
            if (!AllowedDictionaryKeyTypes.Contains(keyType.Type))
            {
                throw new Exception(
                    $"Dictionaries must have strings, enums, or integers as keys, error found in type: {propEntry.TypeName}");
            }
            var valueType = propEntry.CollectionInnerTypes.First(p => !p.IsDictionaryKey);

            if (!valueType.IsPrimitiveType)
            {
                sb.AppendLine(
                    $"\t\t\t\tif (!overrideObj.{GetName(valueType.Type.Name, options.ClassNameConstantsToRemove)}) {{");
                sb.AppendLine(
                    $"\t\t\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key] = new {options.OutputNamespace}.{GetName(valueType.Type.Name, options.ClassNameConstantsToRemove)}(cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key]);");
                sb.AppendLine("\t\t\t\t} else {");
                sb.AppendLine(
                    $"\t\t\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key] = new overrideObj.{GetName(valueType.Type.Name, options.ClassNameConstantsToRemove)}(cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key], overrideObj);");

                sb.AppendLine("\t\t\t\t}");
            }
            else
            {
                sb.AppendLine(
                    $"\t\t\t\tthis.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key] = cons.{ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key];");
            }
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
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
                if (type.IsEnum)
                {
                    var getVals = type.GetEnumNames();
                    var typeName = type.Name;
                    var index = 0;
                    foreach (var enumVal in getVals)
                    {
                        if (generatorOptions.TreatEnumsAsStrings)
                        {
                            propertyTypeCollection.Add(new PropertyBag(typeName, type, enumVal, typeof (string),
                                null, PropertyBag.TransformablePropertyTypeEnum.Primitive, true, enumVal));
                        }
                        else
                        {
                            var trueVal = Convert.ChangeType(Enum.Parse(type, enumVal), type.GetEnumUnderlyingType());
                            propertyTypeCollection.Add(new PropertyBag(typeName, type, enumVal, type.GetEnumUnderlyingType(),
                                null, PropertyBag.TransformablePropertyTypeEnum.Primitive, true, trueVal));
                            //if (int.TryParse(Enum(type, enumVal), out intVal))
                        }

                        index++;
                    }
                    

                }
                else
                {
                    var props = type.GetProperties();
                    var typeName = type.Name;
                    foreach (var prop in props)
                    {
                        if (!ShouldGenerateMember(prop, generatorOptions)) continue;

                        var propertyName = GetPropertyName(prop, generatorOptions);
                        var propertyType = prop.PropertyType;

                        if (!IsPrimitive(propertyType))
                        {
                            if (IsCollectionType(propertyType))
                            {
                                var collectionInnerTypes = GetCollectionInnerTypes(propertyType);
                                var isDictionaryType = IsDictionaryType(propertyType);

                                propertyTypeCollection.Add(new PropertyBag(typeName, type, propertyName, propertyType,
                                    collectionInnerTypes, isDictionaryType
                                        ? PropertyBag.TransformablePropertyTypeEnum.DictionaryType
                                        : PropertyBag.TransformablePropertyTypeEnum.CollectionType, false, null));

                                //if primitive, no need to reflect type
                                if (collectionInnerTypes.All(p => p.IsPrimitiveType)) continue;

                                foreach (var collectionInnerType in collectionInnerTypes.Where(p => !p.IsPrimitiveType))
                                {

                                    if (propertyTypeCollection.All(p => p.TypeName != collectionInnerType.Type.Name))
                                    {
                                        propertyTypeCollection.AddRange(
                                            GetPropertyDictionaryForTypeGeneration(new[] {collectionInnerType.Type},
                                                generatorOptions, propertyTypeCollection));
                                    }
                                }
                            }
                            else
                            {
                                propertyTypeCollection.Add(new PropertyBag(typeName, type, propertyName, propertyType,
                                    null, PropertyBag.TransformablePropertyTypeEnum.ReferenceType, false, null));

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
                                var val = ReadDefaultValueFromAttribute(prop);
                                propertyTypeCollection.Add(new PropertyBag(typeName, type, propertyName, propertyType,
                                    null, PropertyBag.TransformablePropertyTypeEnum.Primitive, true, val));
                            }
                            else
                            {
                                propertyTypeCollection.Add(new PropertyBag(typeName, type, propertyName, propertyType,
                                    null, PropertyBag.TransformablePropertyTypeEnum.Primitive, false, null));
                            }

                            if (propertyType.IsEnum)
                            {
                                if (propertyTypeCollection.All(p => p.TypeName != propertyType.Name))
                                {
                                    propertyTypeCollection.AddRange(
                                        GetPropertyDictionaryForTypeGeneration(new[] {propertyType},
                                            generatorOptions, propertyTypeCollection));
                                }
                            }

                        }
                    }
                }
            }
            return propertyTypeCollection;
        }

        /// <summary>
        /// Gets inner types of collections and dictionaries.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        private static List<PropertyBagTypeInfo> GetCollectionInnerTypes(Type propertyType)
        {
            if (propertyType.IsArray)
            {
                return new List<PropertyBagTypeInfo>()
                {
                    new PropertyBagTypeInfo()
                    {
                        Type = propertyType.GetElementType(),
                        IsPrimitiveType = IsPrimitive(propertyType.GetElementType()),
                        IsEnumType = propertyType.IsEnum
                    }
                };
            }

            if (IsDictionaryType(propertyType))
            {
                return new List<PropertyBagTypeInfo>()
                {
                    new PropertyBagTypeInfo()
                    {
                        Type = propertyType.GetGenericArguments()[0],
                        IsPrimitiveType = IsPrimitive(propertyType.GetGenericArguments()[0]),
                        IsDictionaryKey = true,
                        IsEnumType = propertyType.GetGenericArguments()[0].IsEnum
                    },
                    new PropertyBagTypeInfo()
                    {
                        Type = propertyType.GetGenericArguments()[1],
                        IsPrimitiveType = IsPrimitive(propertyType.GetGenericArguments()[1]),
                        IsEnumType = propertyType.GetGenericArguments()[1].IsEnum
                    }
                };
            }

            return new List<PropertyBagTypeInfo>()
            {
                new PropertyBagTypeInfo()
                {
                    Type =
                        propertyType.GetGenericArguments().Any()
                            ? propertyType.GetGenericArguments()[0]
                            : typeof (string),
                    IsPrimitiveType =
                        IsPrimitive(propertyType.GetGenericArguments().Any()
                            ? propertyType.GetGenericArguments()[0]
                            : typeof (string)),
                    IsEnumType = (propertyType.GetGenericArguments().Any()
                            ? propertyType.GetGenericArguments()[0]
                            : typeof (string)).IsEnum
                }
            };
        }

        /// <summary>
        /// Determines whether the specified property type is a collection.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        private static bool IsCollectionType(Type propertyType)
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
        private static bool IsDictionaryType(Type propertyType)
        {
            return (propertyType.GetInterfaces().Contains(typeof(IDictionary)));
        }

        /// <summary>
        /// Determines whether the specified property type is primitive.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        private static bool IsPrimitive(Type propertyType)
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
        private static bool ShouldGenerateMember(PropertyInfo propertyInfo, JsGeneratorOptions generatorOptions)
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
        private static bool HasDefaultValue(PropertyInfo propertyInfo, JsGeneratorOptions generatorOptions)
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
        private static object ReadDefaultValueFromAttribute(PropertyInfo propertyInfo)
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
