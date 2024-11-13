using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Facteur.MsGraph
{
    public class TokenProvider : IAccessTokenProvider
    {
        public string Token { get; set; }

        public TokenProvider(string token) => Token = token;

        public AllowedHostsValidator AllowedHostsValidator => throw new NotImplementedException();

        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
            => Task.FromResult(Token);
    }
}