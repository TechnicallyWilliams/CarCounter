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
    public class ItemCounterImplTest
    {
        private ItemCounterImpl itemCounter;
        private Mock<IImageClassifier> classifier;

        [TestInitialize]
        public void SetUp()
        {
            this.classifier = new Mock<IImageClassifier>();
            this.itemCounter = new ItemCounterImpl();
            this.itemCounter.Classifier = this.classifier.Object;
        }

        [TestMethod]
        public void TestFullImage()
        {
            this.classifier.Setup(c => c.Classify(It.IsAny<SoftwareBitmap>())).Returns(Task.FromResult("someitem"));
            var frame = LoadTestImage("StoreLogo.png").Result;
            var count = this.itemCounter.CountItems(frame).Result;
            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void TestEmptyImage()
        {
            this.classifier.Setup(c => c.Classify(It.IsAny<SoftwareBitmap>())).Returns(Task.FromResult<string>(null));
            var frame = LoadTestImage("StoreLogo.png").Result;
            var count = this.itemCounter.CountItems(frame).Result;
            Assert.AreEqual(0, count);
        }

        private async Task<SoftwareBitmap> LoadTestImage(string name)
        {
            var path = Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", name);
            var inputFile = await StorageFile.GetFileFromPathAsync(path);
            using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                return await decoder.GetSoftwareBitmapAsync();
            }
        }
    }
}
