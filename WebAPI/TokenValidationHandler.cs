using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Auth10.Swt;

namespace WebAPI
{
    public class TokenValidationHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string token = request.Headers.GetValues("Authorization").First();

            var validator = new SimpleWebTokenValidator
            {
                SharedKeyBase64 = "yourtokensigningkey",
            };

            var swt = validator.ValidateToken(token);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
