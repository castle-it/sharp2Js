using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Castle.Sharp2Js.Tests.DTOs;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Castle.Sharp2Js.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class SerializerTests
    {
        [Test]
        public void TestJilCollectionSerialization()
        {
            var collectionObj = new CollectionTesting()
            {
                ArrayListCollection = new ArrayList() {"Object1", "Object2"},
                CustomListCollection = new CustomListTest<string>() {"Item1", "Item2"},
                DictionaryCollection = new Dictionary<string, string>() {},
                ListCollection = new List<string>() { "Item 1", "Item 2" },
                ObjectArrayCollection = new [] { new CollectionTesting() }
            };

            collectionObj.DictionaryCollection.Add("Key 1", "Value 1");
            collectionObj.DictionaryCollection.Add("Key 2", "Value 2");

            string res = Jil.JSON.Serialize(collectionObj);
        }

        [Test]
        public void TestJilEnumSerialization()
        {
            var collectionObj = new EnumTesting()
            {
                EnumTest1 = EnumTest1.EnumVal1,
                EnumTest2 = EnumTest2.EnumVal2
            };

            

            string res = Jil.JSON.Serialize(collectionObj);
        }

        [Test]
        public void TestNewtonSoftEnumSerialization()
        {
            var collectionObj = new EnumTesting()
            {
                EnumTest1 = EnumTest1.EnumVal2,
                EnumTest2 = EnumTest2.EnumVal2
            };

            

            string res = Newtonsoft.Json.JsonConvert.SerializeObject(collectionObj);
        }
    }
}
