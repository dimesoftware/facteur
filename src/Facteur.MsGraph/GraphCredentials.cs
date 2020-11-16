using System;

namespace Facteur.MsGraph
{
    public readonly struct GraphCredentials
    {
        public GraphCredentials(string clientId, string tenantId, string clientSecret, string from)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientSecret));
            if (string.IsNullOrEmpty(tenantId))
                throw new ArgumentNullException(nameof(clientSecret));
            if (string.IsNullOrEmpty(clientSecret))
                throw new ArgumentNullException(nameof(clientSecret));
            if (string.IsNullOrEmpty(from))
                throw new ArgumentNullException(nameof(clientSecret));

            ClientId = clientId;
            TenantId = tenantId;
            ClientSecret = clientSecret;
            From = from;
        }

        public string ClientId { get; }
        public string TenantId { get; }
        public string ClientSecret { get; }
        public string From { get; }
    }
}