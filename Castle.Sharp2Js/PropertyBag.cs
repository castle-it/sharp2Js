using System;

namespace Castle.Sharp2Js
{
    public class PropertyBag
    {
        public PropertyBag()
        {

        }

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

        public string TypeName { get; set; }
        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }
        public bool IsArray { get; set; }
        public string PropertyTypeName { get; set; }
        public bool IsPrimitiveType { get; set; }

    }
}
