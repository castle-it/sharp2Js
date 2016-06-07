using System;
using System.Collections.Generic;
using System.Linq;

namespace Castle.Sharp2Js
{
    /// <summary>
    /// Generates a list of property type information to be used by the Js Generator
    /// </summary>
    public static class TypePropertyDictionaryGenerator
    {
        /// <summary>
        /// Gets the property dictionary to be used for type generation.
        /// </summary>
        /// <param name="types">The types to generate property information for.</param>
        /// <param name="generatorOptions">The generator options.</param>
        /// <param name="propertyTypeCollection">The output collection of properties discovered through reflection of the supplied classes.</param>
        /// <returns></returns>
        public static IEnumerable<PropertyBag> GetPropertyDictionaryForTypeGeneration(IEnumerable<Type> types,
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
                        }
                    }
                }
                else
                {
                    var props = type.GetProperties();
                    var typeName = type.Name;
                    foreach (var prop in props)
                    {
                        if (!Helpers.ShouldGenerateMember(prop, generatorOptions)) continue;

                        var propertyName = Helpers.GetPropertyName(prop, generatorOptions);
                        var propertyType = prop.PropertyType;

                        if (!Helpers.IsPrimitive(propertyType))
                        {
                            if (Helpers.IsCollectionType(propertyType))
                            {
                                var collectionInnerTypes = GetCollectionInnerTypes(propertyType);
                                var isDictionaryType = Helpers.IsDictionaryType(propertyType);

                                propertyTypeCollection.Add(new PropertyBag(typeName, type, propertyName, propertyType,
                                    collectionInnerTypes, isDictionaryType
                                        ? PropertyBag.TransformablePropertyTypeEnum.DictionaryType
                                        : PropertyBag.TransformablePropertyTypeEnum.CollectionType, false, null));

                                //if primitive, no need to reflect type
                                if (collectionInnerTypes.All(p => p.IsPrimitiveType)) continue;

                                foreach (var collectionInnerType in collectionInnerTypes.Where(p => !p.IsPrimitiveType))
                                {
                                    var innerTypeName = collectionInnerType.Type.Name;
                                    if (propertyTypeCollection.All(p => p.TypeName != innerTypeName))
                                    {
                                        GetPropertyDictionaryForTypeGeneration(new[] {collectionInnerType.Type},
                                                generatorOptions, propertyTypeCollection);
                                    }
                                }
                            }
                            else
                            {
                                propertyTypeCollection.Add(new PropertyBag(typeName, type, propertyName, propertyType,
                                    null, PropertyBag.TransformablePropertyTypeEnum.ReferenceType, false, null));

                                if (propertyTypeCollection.All(p => p.TypeName != propertyType.Name))
                                {
                                    GetPropertyDictionaryForTypeGeneration(new[] {propertyType},
                                            generatorOptions, propertyTypeCollection);
                                }
                            }
                        }
                        else
                        {
                            var hasDefaultValue = Helpers.HasDefaultValue(prop, generatorOptions);
                            if (hasDefaultValue)
                            {
                                var val = Helpers.ReadDefaultValueFromAttribute(prop);
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
                                    GetPropertyDictionaryForTypeGeneration(new[] {propertyType},
                                            generatorOptions, propertyTypeCollection);
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
                        IsPrimitiveType = Helpers.IsPrimitive(propertyType.GetElementType()),
                        IsEnumType = propertyType.IsEnum
                    }
                };
            }

            if (Helpers.IsDictionaryType(propertyType))
            {
                return new List<PropertyBagTypeInfo>()
                {
                    new PropertyBagTypeInfo()
                    {
                        Type = propertyType.GetGenericArguments()[0],
                        IsPrimitiveType = Helpers.IsPrimitive(propertyType.GetGenericArguments()[0]),
                        IsDictionaryKey = true,
                        IsEnumType = propertyType.GetGenericArguments()[0].IsEnum
                    },
                    new PropertyBagTypeInfo()
                    {
                        Type = propertyType.GetGenericArguments()[1],
                        IsPrimitiveType = Helpers.IsPrimitive(propertyType.GetGenericArguments()[1]),
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
                    IsPrimitiveType = Helpers.IsPrimitive(propertyType.GetGenericArguments().Any()
                        ? propertyType.GetGenericArguments()[0]
                        : typeof (string)),
                    IsEnumType = (propertyType.GetGenericArguments().Any()
                        ? propertyType.GetGenericArguments()[0]
                        : typeof (string)).IsEnum
                }
            };
        }
    }
}
