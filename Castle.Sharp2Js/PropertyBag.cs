using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Castle.Sharp2Js
{
    /// <summary>
    /// Responsible for storing data about the type models to be generated
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PropertyBag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBag" /> class.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="collectionInnerTypes">The collection inner types.</param>
        /// <param name="transformablePropertyType">Type of the transformable property.</param>
        /// <param name="hasDefaultValue">if set to <c>true</c> [has default value].</param>
        /// <param name="defaultValue">The default value.</param>
        public PropertyBag(string typeName, Type typeDefinition, string propertyName, Type propertyType,
            List<PropertyBagTypeInfo> collectionInnerTypes, 
            TransformablePropertyTypeEnum transformablePropertyType,
            bool hasDefaultValue, object defaultValue)
        {
            TypeName = typeName;
            PropertyName = propertyName;
            PropertyType = propertyType;
            CollectionInnerTypes = collectionInnerTypes;
            HasDefaultValue = hasDefaultValue;
            DefaultValue = defaultValue;
            TransformablePropertyType = transformablePropertyType;
            TypeDefinition = typeDefinition;
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
        /// Determines what class of object the transformer treats the object as 
        /// which determines how the pieces are treated when writing Js objects.
        /// </summary>
        /// <value>
        /// The type of the transformable property.
        /// </value>
        public TransformablePropertyTypeEnum TransformablePropertyType { get; set; }
        /// <summary>
        /// Gets or sets the name of the property type.
        /// </summary>
        /// <value>
        /// The name of the property type.
        /// </value>
        public List<PropertyBagTypeInfo> CollectionInnerTypes { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance has a default value.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has default value; otherwise, <c>false</c>.
        /// </value>
        public bool HasDefaultValue { get; set; }
        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public object DefaultValue { get; set; }
        /// <summary>
        /// Gets or sets the type definition.
        /// </summary>
        /// <value>
        /// The type definition.
        /// </value>
        public Type TypeDefinition { get; set; }

        /// <summary>
        /// Transformable property types understood by sharp2Js
        /// </summary>
        public enum TransformablePropertyTypeEnum
        {
            /// <summary>
            /// The primitive
            /// </summary>
            Primitive = 1,
            /// <summary>
            /// The collection type
            /// </summary>
            CollectionType = 2,
            /// <summary>
            /// The dictionary type
            /// </summary>
            DictionaryType = 3,
            /// <summary>
            /// The reference type
            /// </summary>
            ReferenceType = 4
        }
    }
    /// <summary>
    /// Contains types contained within collections and designations for dictionary types
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PropertyBagTypeInfo
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type Type { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is the dictionary key.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is the dictionary key; otherwise, <c>false</c>.
        /// </value>
        public bool IsDictionaryKey { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is primitive type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is primitive type; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrimitiveType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enum type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is enum type; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnumType { get; set; }
    }
    
}
