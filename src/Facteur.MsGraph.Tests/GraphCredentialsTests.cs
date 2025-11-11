using System;
using System.Diagnostics.CodeAnalysis;
using Facteur.MsGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Facteur.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class GraphCredentialsTests
    {
        [TestMethod]
        public void GraphCredentials_Constructor_CorrectParameters_ShouldSetProperties()
        {
            GraphCredentials credentials = new("client", "tenant", "secret", "from");

            Assert.AreEqual("client", credentials.ClientId);
            Assert.AreEqual("tenant", credentials.TenantId);
            Assert.AreEqual("secret", credentials.ClientSecret);
            Assert.AreEqual("from", credentials.From);
        }

        [TestMethod]
        public void GraphCredentials_Constructor_HasMissingClientId_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new GraphCredentials("", "tenant", "secret", "from"));
        }

        [TestMethod]
        public void GraphCredentials_Constructor_HasMissingTenantId_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new GraphCredentials("client", "", "secret", "from"));
        }

        [TestMethod]
        public void GraphCredentials_Constructor_HasMissingSecret_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new GraphCredentials("client", "tenant", "", "from"));
        }

        [TestMethod]
        public void GraphCredentials_Constructor_HasMissingFrom_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new GraphCredentials("client", "tenant", "secret", ""));
        }
    }
}