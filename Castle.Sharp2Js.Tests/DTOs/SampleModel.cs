using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Castle.Sharp2Js.Tests.DTOs
{
    public class RecursiveTest
    {
        public string Name { get; set; }
        public List<RecursiveTest> RecursiveTests { get; set; }
    }

    public class ArrayTypeTest
    {
        public string[] Strings { get; set; }
    }

    public class AttributeInformationTest
    {
        [DataMember(Name = "TestName")]
        public string TypeName { get; set; }
        [IgnoreDataMember]
        public bool IgnoreMe { get; set; }
    }

}
