using System;
using System.Collections.Generic;
using System.Linq;
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
            var propertyClassCollection = TypePropertyDictionaryGenerator.GetPropertyDictionaryForTypeGeneration(typesToGenerate, passedOptions);
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
            var propertyDictionary = TypePropertyDictionaryGenerator.GetPropertyDictionaryForTypeGeneration(new[] { modelType }, Options);

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

                if (options.IncludeEqualsFunction && !typeDefinition.IsEnum)
                {
                    sb.AppendLine();
                    BuildEqualsFunctionForClass(sb, propList, options);
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
                    $"{options.OutputNamespace}.{Helpers.GetName(type.First().TypeName, options.ClassNameConstantsToRemove)} = function (cons, overrideObj) {{");
                sb.AppendLine("\tif (!overrideObj) { overrideObj = { }; }");
                sb.AppendLine("\tif (!cons) { cons = { }; }");
            }
            else if (type.First().TypeDefinition.IsEnum)
            {
                sb.AppendLine(
                    $"{options.OutputNamespace}.{Helpers.GetName(type.First().TypeName, options.ClassNameConstantsToRemove)} = {{");
            }
            else
            {
                sb.AppendLine(
                    $"{options.OutputNamespace}.{Helpers.GetName(type.First().TypeName, options.ClassNameConstantsToRemove)} = function (cons) {{");

                sb.AppendLine("\tif (!cons) { cons = { }; }");
            }
            
        }

        /// <summary>
        /// Builds the equals function for a type.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="propList">The property list.</param>
        /// <param name="options">The options.</param>
        private static void BuildEqualsFunctionForClass(StringBuilder sb, IEnumerable<PropertyBag> propList,
            JsGeneratorOptions options)
        {
            //Generate an equals function for two objects
            sb.AppendLine("\tthis.$equals = function (compareObj) {");
            sb.AppendLine("\t\tif (!compareObj) { return false; }");
            foreach (var propEntry in propList)
            {
                switch (propEntry.TransformablePropertyType)
                {
                    case PropertyBag.TransformablePropertyTypeEnum.CollectionType:
                        sb.AppendLine(
                            $"\t\tif (compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} !== this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine($"\t\t\tif (!compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine($"\t\t\t\treturn false;");
                        sb.AppendLine($"\t\t\t}}");
                        sb.AppendLine($"\t\t\tif (!this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine($"\t\t\t\treturn false;");
                        sb.AppendLine($"\t\t\t}}");
                        sb.AppendLine($"\t\t\tif (compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}.length != this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}.length) {{");
                        sb.AppendLine($"\t\t\t\treturn false;");
                        sb.AppendLine($"\t\t\t}}");
                        sb.AppendLine(
                            $"\t\t\tfor (i = 0; i < this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}.length; i++) {{");
                        var collectionType = propEntry.CollectionInnerTypes.First();

                        if (!collectionType.IsPrimitiveType)
                        {
                            sb.AppendLine(
                                $"\t\t\t\tif (!this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i].$equals(compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i])) {{ return false; }};");

                        }
                        else
                        {
                            sb.AppendLine(
                                $"\t\t\t\tif (this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i] !== compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i]) {{ return false; }};");
                        }
                        sb.AppendLine($"\t\t\t}}");
                        sb.AppendLine($"\t\t}}");
                        
                        break;
                    case PropertyBag.TransformablePropertyTypeEnum.DictionaryType:
                        sb.AppendLine(
                            $"\t\tif (compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} !== this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine($"\t\t\tif (!compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine($"\t\t\t\treturn false;");
                        sb.AppendLine($"\t\t\t}}");
                        sb.AppendLine($"\t\t\tif (!this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine($"\t\t\t\treturn false;");
                        sb.AppendLine($"\t\t\t}}");
                        /*sb.AppendLine(
                            $"\t\tif (this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} != null) {{");*/
                        sb.AppendLine(
                            $"\t\t\tfor (var key in this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine(
                            $"\t\t\t\tif (!compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}.hasOwnProperty(key)) {{");
                        sb.AppendLine(
                            $"\t\t\t\t\treturn false;");
                        sb.AppendLine("\t\t\t\t}");
                        var valueType = propEntry.CollectionInnerTypes.First(p => !p.IsDictionaryKey);

                        if (!valueType.IsPrimitiveType)
                        {
                            sb.AppendLine(
                                $"\t\t\t\tif (!this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key].$equals(compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key])) {{ return false; }};");
                        }
                        else
                        {
                            sb.AppendLine(
                                $"\t\t\t\tif (this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key] !== compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key]) {{ return false; }};");
                        }
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine("\t\t}");
                        break;
                    case PropertyBag.TransformablePropertyTypeEnum.ReferenceType:
                        sb.AppendLine(
                            $"\t\tif (compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} !== this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine($"\t\t\tif (!compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine($"\t\t\t\treturn false;");
                        sb.AppendLine($"\t\t\t}}");
                        sb.AppendLine($"\t\t\tif (!this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine($"\t\t\t\treturn false;");
                        sb.AppendLine($"\t\t\t}}");
                        sb.AppendLine(
                                $"\t\t\tif (!this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}.$equals(compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)})) {{ return false; }};");
                        sb.AppendLine("\t\t}");
                        break;
                    case PropertyBag.TransformablePropertyTypeEnum.Primitive:
                        sb.AppendLine(
                            $"\t\tif (this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} !== compareObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{ return false; }};");
                        break;
                }
            }
            sb.AppendLine("\treturn true;");
            sb.AppendLine("\t}");
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
                            $"\t\tif (!mergeObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine($"\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} = [];");
                        sb.AppendLine("\t\t}");
                        sb.AppendLine(
                            $"\t\tif (this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} != null) {{");
                        sb.AppendLine(string.Format("\t\t\tthis.{0}.splice(0, this.{0}.length);", Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)));
                        sb.AppendLine("\t\t}");
                        sb.AppendLine(
                            $"\t\tif (mergeObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine(
                            $"\t\t\tif (this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} === null) {{");
                        sb.AppendLine($"\t\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} = [];");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine(
                            $"\t\t\tfor (i = 0; i < mergeObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}.length; i++) {{");
                        sb.AppendLine(string.Format("\t\t\t\tthis.{0}.push(mergeObj.{0}[i]);", Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)));
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine("\t\t}");
                        break;
                    case PropertyBag.TransformablePropertyTypeEnum.DictionaryType:
                        sb.AppendLine(
                            $"\t\tif (this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} != null) {{");
                        sb.AppendLine(
                            $"\t\t\tfor (var key in this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine(
                            $"\t\t\t\tif (this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}.hasOwnProperty(key)) {{");
                        sb.AppendLine(
                            $"\t\t\t\t\tdelete this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key];");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine("\t\t}");
                        sb.AppendLine(
                            $"\t\tif (mergeObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine(
                            $"\t\t\tfor (var key in mergeObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                        sb.AppendLine(
                            $"\t\t\t\tif (mergeObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}.hasOwnProperty(key)) {{");
                        sb.AppendLine(
                            $"\t\t\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key] = mergeObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key];");
                        sb.AppendLine("\t\t\t\t}");
                        sb.AppendLine("\t\t\t}");
                        sb.AppendLine("\t\t}");
                        break;
                    case PropertyBag.TransformablePropertyTypeEnum.ReferenceType:
                        sb.AppendLine(
                            $"\t\tif (mergeObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} == null) {{");
                        sb.AppendLine($"\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} = null;");
                        sb.AppendLine(
                            $"\t\t}} else if (this.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} != null) {{");
                        sb.AppendLine(
                            $"\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}.$merge(mergeObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)});");
                        sb.AppendLine("\t\t} else {");
                        sb.AppendLine(
                            $"\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} = mergeObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)};");
                        sb.AppendLine("\t\t}");
                        break;
                    case PropertyBag.TransformablePropertyTypeEnum.Primitive:
                        sb.AppendLine(
                            $"\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} = mergeObj.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)};");
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
                        ? $"\t{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}: '{propEntry.DefaultValue}',"
                        : $"\t{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}: {propEntry.DefaultValue},");
            }
            else if (propEntry.HasDefaultValue)
            {
                var writtenValue = propEntry.DefaultValue is bool
                    ? propEntry.DefaultValue.ToString().ToLower()
                    : propEntry.DefaultValue;
                sb.AppendLine(
                    $"\tif (!cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
                sb.AppendLine(
                    propEntry.PropertyType == typeof(string)
                        ? $"\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} = '{writtenValue}';"
                        : $"\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} = {writtenValue};");
                sb.AppendLine("\t} else {");
                sb.AppendLine(
                    $"\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} = cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)};");
                sb.AppendLine("\t}");
            }
            else
            {
                sb.AppendLine(
                    $"\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} = cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)};");
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

            sb.AppendLine($"\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} = null;");
            sb.AppendLine($"\tif (cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
            sb.AppendLine(
                $"\t\tif (!overrideObj.{Helpers.GetName(propEntry.PropertyType.Name, options.ClassNameConstantsToRemove)}) {{");
            sb.AppendLine(
                $"\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} = new {options.OutputNamespace}.{Helpers.GetName(propEntry.PropertyType.Name, options.ClassNameConstantsToRemove)}(cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)});");
            sb.AppendLine("\t\t} else {");
            sb.AppendLine(
                $"\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} = new overrideObj.{Helpers.GetName(propEntry.PropertyType.Name, options.ClassNameConstantsToRemove)}(cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}, overrideObj);");

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
            sb.AppendLine(string.Format("\tthis.{0} = new Array(cons.{0} == null ? 0 : cons.{1}.length );", Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase), Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)));
            sb.AppendLine($"\tif(cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} != null) {{");
            sb.AppendLine(
                $"\t\tfor (i = 0, length = cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}.length; i < length; i++) {{");

            var collectionType = propEntry.CollectionInnerTypes.First();

            if (!collectionType.IsPrimitiveType)
            {
                sb.AppendLine(
                    $"\t\t\tif (!overrideObj.{Helpers.GetName(collectionType.Type.Name, options.ClassNameConstantsToRemove)}) {{");
                sb.AppendLine(
                    $"\t\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i] = new {options.OutputNamespace}.{Helpers.GetName(collectionType.Type.Name, options.ClassNameConstantsToRemove)}(cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i]);");
                sb.AppendLine("\t\t\t} else {");
                sb.AppendLine(
                    $"\t\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i] = new overrideObj.{Helpers.GetName(collectionType.Type.Name, options.ClassNameConstantsToRemove)}(cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i], overrideObj);");

                sb.AppendLine("\t\t\t}");
            }
            else
            {
                sb.AppendLine(
                    $"\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i] = cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[i];");
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
            sb.AppendLine($"\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} = {{}};");
            sb.AppendLine($"\tif(cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)} != null) {{");
            sb.AppendLine(
                $"\t\tfor (var key in cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}) {{");
            sb.AppendLine(
                $"\t\t\tif (cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}.hasOwnProperty(key)) {{");

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
                    $"\t\t\t\tif (!overrideObj.{Helpers.GetName(valueType.Type.Name, options.ClassNameConstantsToRemove)}) {{");
                sb.AppendLine(
                    $"\t\t\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key] = new {options.OutputNamespace}.{Helpers.GetName(valueType.Type.Name, options.ClassNameConstantsToRemove)}(cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key]);");
                sb.AppendLine("\t\t\t\t} else {");
                sb.AppendLine(
                    $"\t\t\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key] = new overrideObj.{Helpers.GetName(valueType.Type.Name, options.ClassNameConstantsToRemove)}(cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key], overrideObj);");

                sb.AppendLine("\t\t\t\t}");
            }
            else
            {
                sb.AppendLine(
                    $"\t\t\t\tthis.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key] = cons.{Helpers.ToCamelCase(propEntry.PropertyName, options.CamelCase)}[key];");
            }
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
        }
    }
}
