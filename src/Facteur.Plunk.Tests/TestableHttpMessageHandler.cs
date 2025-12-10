using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Facteur.Tests
{
    /// <summary>
    /// Testable HttpMessageHandler for use with NSubstitute
    /// </summary>
    public class TestableHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage _response;
        private Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public void SetResponse(HttpResponseMessage response)
        {
            _response = response;
        }

        public void SetHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_handler != null)
                return _handler(request, cancellationToken);

            return Task.FromResult(_response ?? new HttpResponseMessage());
        }
    }
}

