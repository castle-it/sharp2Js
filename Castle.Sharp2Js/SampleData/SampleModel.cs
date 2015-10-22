using System.Collections.Generic;

namespace Castle.Sharp2Js.SampleData
{
    public class AddressInformation
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int ZipCode { get; set; }
        public OwnerInformation Owner { get; set; }
        public List<Feature> Features { get; set; }
        public List<string> Tags { get; set; }
    }

    public class OwnerInformation
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    public class Feature
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }
}
