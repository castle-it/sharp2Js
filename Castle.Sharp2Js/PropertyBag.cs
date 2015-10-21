using System;

namespace Castle.Sharp2Js
{
    public class PropertyBag
    {
        public PropertyBag()
        {

        }

        public PropertyBag(string typeName, string propertyName, Type propertyType,
            bool isGenericList, string propertyTypeName, bool isPrimitiveType)
        {
            TypeName = typeName;
            PropertyName = propertyName;
            PropertyType = propertyType;
            IsGenericList = isGenericList;
            PropertyTypeName = propertyTypeName;
            IsPrimitiveType = isPrimitiveType;
        }

        public string TypeName { get; set; }
        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }
        public bool IsGenericList { get; set; }
        public string PropertyTypeName { get; set; }
        public bool IsPrimitiveType { get; set; }

    }
}
