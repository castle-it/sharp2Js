using System;
using Castle.Sharp2Js.Tests.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Castle.Sharp2Js.Tests
{
    [TestClass]
    public class JsGeneratorTests
    {
        [TestMethod]
        public void BasicGeneration()
        {
            //Generate a basic javascript model from a C# class

            var modelType = typeof (AddressInformation);

            var outputJs = JsGenerator.GenerateJsModelFromTypeWithDescendants(modelType, true, "castle");

            Assert.IsTrue(!string.IsNullOrEmpty(outputJs));

            var js = new Jint.Parser.JavaScriptParser();

            try
            {
                js.Parse(outputJs);
            }
            catch (Exception ex)
            {
                Assert.Fail("Expected no exception parsing javascript, but got: " + ex.Message);
            }

            
        }
    }
}
