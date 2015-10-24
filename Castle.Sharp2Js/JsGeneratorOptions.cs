using System.Collections.Generic;

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
        public bool CamelCase { get; set; }
        /// <summary>
        /// Gets or sets the output namespace of the javascript objects.
        /// </summary>
        /// <value>
        /// The output namespace.
        /// </value>
        public string OutputNamespace { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to include a merge function for the js objects.
        /// </summary>
        /// <value>
        /// <c>true</c> if [include merge function]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeMergeFunction { get; set; }
        /// <summary>
        /// Gets or sets a list of strings to remove from class names (e.g. Dto) automatically.
        /// </summary>
        /// <value>
        /// The class name constants to remove.
        /// </value>
        public List<string> ClassNameConstantsToRemove { get; set; }
    }
}
