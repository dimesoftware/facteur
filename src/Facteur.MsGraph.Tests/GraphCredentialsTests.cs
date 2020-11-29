using System;
using Facteur.MsGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Tests
{
    [TestClass]
    public class GraphCredentialsTests
    {
        [TestMethod]
        public void GraphCredentials_Constructor_CorrectParameters_ShouldSetProperties()
        {
            GraphCredentials credentials = new GraphCredentials("client", "tenant", "secret", "from");

            Assert.IsTrue(credentials.ClientId == "client");
            Assert.IsTrue(credentials.TenantId == "tenant");
            Assert.IsTrue(credentials.ClientSecret == "secret");
            Assert.IsTrue(credentials.From == "from");
        }

        [TestMethod]
        public void GraphCredentials_Constructor_HasMissingClientId_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new GraphCredentials("", "tenant", "secret", "from"));
        }

        [TestMethod]
        public void GraphCredentials_Constructor_HasMissingTenantId_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new GraphCredentials("client", "", "secret", "from"));
        }

        [TestMethod]
        public void GraphCredentials_Constructor_HasMissingSecret_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new GraphCredentials("client", "tenant", "", "from"));
        }

        [TestMethod]
        public void GraphCredentials_Constructor_HasMissingFrom_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new GraphCredentials("client", "tenant", "secret", ""));
        }
    }
}