using System;
using System.Threading.Tasks;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Storage;
using System.IO;

namespace Vision.Test
{
    [TestClass]
    public class HttpImageClassifierTest
    {
        private HttpImageClassifier classifier;

        [TestInitialize]
        public void SetUp()
        {
            this.classifier = new HttpImageClassifier();
        }

        [TestMethod]
        public void TestGetPredictionEmpty()
        {
            var result = this.classifier.GetPrediction("{\"predictions\":[]}");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TestGetPredictionSingle()
        {
            var result = this.classifier.GetPrediction("{\"predictions\" : [ { \"probability\":0.60, \"tagName\": \"foo\" } ] }");
            Assert.AreEqual("foo", result);
        }

        [TestMethod]
        public void TestGetPredictionNoDash()
        {
            var result = this.classifier.GetPrediction("{\"predictions\" : [ { \"probability\":0.60, \"tagName\": \"no-foo\" } ] }");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TestGetPredictionMultipleUnordered()
        {
            var result = this.classifier.GetPrediction("{\"predictions\" : [ { \"probability\":0.20, \"tagName\": \"foo\" }, { \"probability\":0.60, \"tagName\": \"bar\" }, { \"probability\":0.30, \"tagName\": \"baz\" } ] }");
            Assert.AreEqual("bar", result);
        }

        [TestMethod]
        public void TestGetPredictionAllBelowThreshold()
        {
            var result = this.classifier.GetPrediction("{\"predictions\" : [ { \"probability\":0.20, \"tagName\": \"foo\" }, { \"probability\":0.30, \"tagName\": \"bar\" }, { \"probability\":0.40, \"tagName\": \"baz\" } ] }");
            Assert.IsNull(result);
        }

    }
}
