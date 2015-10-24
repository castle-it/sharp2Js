using System;
using System.Collections.Generic;
using System.Linq;
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

}
