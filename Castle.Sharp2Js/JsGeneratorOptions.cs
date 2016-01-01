using System;
using System.Collections.Generic;
using System.Text;

namespace Castle.Sharp2Js
{
    /// <summary>
    /// Provides details about behaviors and output configurations used when generating Js from C# classes.
    /// </summary>
    public class JsGeneratorOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to camel case the property names.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [camel case]; otherwise, <c>false</c>.
        /// </value>
        public bool CamelCase { get; set; } = false;

        /// <summary>
        /// Gets or sets the output namespace of the javascript objects.
        /// </summary>
        /// <value>
        /// The output namespace.
        /// </value>
        public string OutputNamespace { get; set; } = "models";

        /// <summary>
        /// Gets or sets a value indicating whether to include a merge function for the js objects.
        /// </summary>
        /// <value>
        /// <c>true</c> if [include merge function]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeMergeFunction { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include an optimized equals function for the js objects.
        /// </summary>
        /// <value>
        /// <c>true</c> if [include equals function]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeEqualsFunction { get; set; } = false;
        /// <summary>
        /// Gets or sets a list of strings to remove from class names (e.g. Dto) automatically.
        /// </summary>
        /// <value>
        /// The class name constants to remove.
        /// </value>
        public List<string> ClassNameConstantsToRemove { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether to respect the DataMember name attribute when present.
        /// </summary>
        /// <value>
        /// <c>true</c> if [respect data member attribute]; otherwise, <c>false</c>.
        /// </value>
        public bool RespectDataMemberAttribute { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to respect the DefaultValue attribute when present.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [respect default value attribute]; otherwise, <c>false</c>.
        /// </value>
        public bool RespectDefaultValueAttribute { get; set; } = true;

        /// <summary>
        /// Gets or sets the custom function processors that can be run per type.
        /// </summary>
        /// <value>
        /// The custom function processors.
        /// </value>
        public List<Action<StringBuilder, IEnumerable<PropertyBag>, JsGeneratorOptions>> CustomFunctionProcessors { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enum values should be treated as strings (the default for serializers like Jil) instead of ints.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [treat enums as strings]; otherwise, <c>false</c>.
        /// </value>
        public bool TreatEnumsAsStrings { get; set; }
    }
}
