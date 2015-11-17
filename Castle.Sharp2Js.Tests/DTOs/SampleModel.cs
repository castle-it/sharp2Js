using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Castle.Sharp2Js.Tests.DTOs
{
    [ExcludeFromCodeCoverage]
    public class RecursiveTest
    {
        public string Name { get; set; }
        public List<RecursiveTest> RecursiveTests { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class ArrayTypeTest
    {
        public string[] Strings { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class AttributeInformationTest
    {
        [DataMember(Name = "TestName")]
        public string TypeName { get; set; }
        [IgnoreDataMember]
        public bool IgnoreMe { get; set; }
        [DefaultValue(23.19)]
        public double NumberValue1 { get; set; }

        [DefaultValue("HelloWorld")]
        public string StringValue1 { get; set; }

        [DataMember(Name = " ")]
        public string InvalidName1 { get; set; }

        public List<StructTest> StructTests { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public struct StructTest
    {
        public int SructValue { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class CamelCaseTest
    {
        public string WKTPolygon { get; set; }
        public string alreadyUnderscored { get; set; }
        public string RegularCased { get; set; }
    }

}
