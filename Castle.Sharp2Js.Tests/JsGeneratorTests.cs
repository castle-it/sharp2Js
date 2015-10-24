using System;
using System.Collections.Generic;
using Castle.Sharp2Js.SampleData;
using Castle.Sharp2Js.Tests.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Castle.Sharp2Js.Tests
{
    [TestClass]
    public class JsGeneratorTests
    {
        [TestMethod]
        public void BasicGenerationLegacy()
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

        [TestMethod]
        public void RecursiveTypeGenerationLegacy()
        {
            //Generate a basic javascript model from a C# class

            var modelType = typeof(RecursiveTest);

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

        [TestMethod]
        public void BasicGeneration()
        {
            //Generate a basic javascript model from a C# class

            var modelType = typeof(AddressInformation);

            var outputJs = JsGenerator.Generate(new[] {modelType});

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

        [TestMethod]
        public void BasicGenerationWithOptions()
        {
            //Generate a basic javascript model from a C# class

            var modelType = typeof(AddressInformation);

            var outputJs = JsGenerator.Generate(new[] { modelType }, new JsGeneratorOptions()
            {
                ClassNameConstantsToRemove = new List<string>() { "Dto" },
                CamelCase = true,
                IncludeMergeFunction = false,
                OutputNamespace = "models"
            });

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

        [TestMethod]
        public void ArrayTypeHandling()
        {
            //Generate a basic javascript model from a C# class

            var modelType = typeof(ArrayTypeTest);

            var outputJs = JsGenerator.Generate(new[] { modelType }, new JsGeneratorOptions()
            {
                ClassNameConstantsToRemove = new List<string>() { "Dto" },
                CamelCase = true,
                IncludeMergeFunction = false,
                OutputNamespace = "models"
            });

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
