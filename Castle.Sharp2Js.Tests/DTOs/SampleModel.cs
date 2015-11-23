using System;
using System.Collections;
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
    [ExcludeFromCodeCoverage]
    public class CustomListTest<T> : IList
    {
        private List<T> _privateList = new List<T>();
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _privateList.Count; }
        }

        public object SyncRoot
        {
            get { return _privateList; }
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        public int Add(object value)
        {
            _privateList.Add((T)value);
            return _privateList.Count;
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public object this[int index]
        {
            get { return _privateList[index]; }
            set { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }
    }

    [ExcludeFromCodeCoverage]
    public class CollectionTesting
    {
        public List<string> ListCollection { get; set; }

        public CollectionTesting[] ObjectArrayCollection { get; set; }

        public ArrayList ArrayListCollection { get; set; }

        public Dictionary<string, string> DictionaryCollection { get; set; }

        public Dictionary<string, ArrayTypeTest> DictionaryObjectCollection { get; set; }

        public CustomListTest<string> CustomListCollection { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class DictionaryKeyTesting
    {
        public Dictionary<ArrayTypeTest, string> DictionaryObjectKeyCollection { get; set; }

    }



}
