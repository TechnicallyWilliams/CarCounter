using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Vision.Test
{
    [TestClass]
    public class LabelCountDAOTest
    {
        private LabelCountDAO labelCountDAO;
        private Mock<HttpMessageHandler> handler = new Mock<HttpMessageHandler>();
        private Mock<IHttpHandler> iHttpHandler;

        [TestInitialize]
        public void SetUp()
        {
            iHttpHandler = new Mock<IHttpHandler>(MockBehavior.Loose);

            iHttpHandler.Setup(x => x.PostAsync(It.IsAny<Uri>(), It.IsAny<StringContent>()))
                .Returns(Task.FromResult<HttpResponseMessage>(null));

            this.labelCountDAO = new LabelCountDAO();
            this.labelCountDAO.httpHandler = new Lazy<IHttpHandler>(() => iHttpHandler.Object);
        }

        //Integration Test
        [TestMethod]
        public async Task LabelCountDAOSave() 
        {
            //setup
            LabelCount label = new LabelCount();
            label.TimeStamp = DateTime.UtcNow;
            label.Label = "IntegrationTest";
            label.Count = 11;

            //act
           await labelCountDAO.Save(label);

            // assert
            this.iHttpHandler.Verify(x => x.PostAsync(It.IsAny<Uri>(), It.IsAny<StringContent>()), Times.AtLeastOnce);
        }

    }
}
