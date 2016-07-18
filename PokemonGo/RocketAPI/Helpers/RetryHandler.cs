using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Helpers
{
    class RetryHandler : DelegatingHandler
    {
        private const int MaxRetries = 10;

        public RetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        { }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i <= MaxRetries; i++)
            {
                try
                {
                    return await base.SendAsync(request, cancellationToken);
                }
                catch (Exception ex)
                {
                    if (i < MaxRetries)
                    {
                        await Task.Delay(2000);
                        continue;
                    }
                    throw;
                }
            }
            return null;
        }
    }
}
