using System;

namespace Castle.Sharp2Js
{
    /// <summary>
    /// Responsible for storing data about the type models to be generated
    /// </summary>
    public class PropertyBag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBag"/> class.
        /// </summary>
        public PropertyBag()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBag"/> class.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="isArray">if set to <c>true</c> [is array].</param>
        /// <param name="propertyTypeName">Name of the property type.</param>
        /// <param name="isPrimitiveType">if set to <c>true</c> [is primitive type].</param>
        public PropertyBag(string typeName, string propertyName, Type propertyType,
            bool isArray, string propertyTypeName, bool isPrimitiveType)
        {
            TypeName = typeName;
            PropertyName = propertyName;
            PropertyType = propertyType;
            IsArray = isArray;
            PropertyTypeName = propertyTypeName;
            IsPrimitiveType = isPrimitiveType;
        }

        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        /// <value>
        /// The name of the type.
        /// </value>
        public string TypeName { get; set; }
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; set; }
        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>
        /// The type of the property.
        /// </value>
        public Type PropertyType { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is an array.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is an array; otherwise, <c>false</c>.
        /// </value>
        public bool IsArray { get; set; }
        /// <summary>
        /// Gets or sets the name of the property type.
        /// </summary>
        /// <value>
        /// The name of the property type.
        /// </value>
        public string PropertyTypeName { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is primitive type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is primitive type; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrimitiveType { get; set; }

    }
}
