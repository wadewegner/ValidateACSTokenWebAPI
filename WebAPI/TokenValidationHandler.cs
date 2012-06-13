using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Auth10.Swt;
using System.Web;

namespace WebAPI
{
    public class TokenValidationHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string authHeader = request.Headers.GetValues("Authorization").First();

            string header = "OAuth ";
            string token = string.Empty;

            if (string.CompareOrdinal(authHeader, 0, header, 0, header.Length) == 0)
            {
                token = authHeader.Remove(0, header.Length);
            }
            else
            {
                throw new HttpException((int)System.Net.HttpStatusCode.Unauthorized, "The authorization header was invalid");
            }

            var validator = new SimpleWebTokenValidator
            {
                SharedKeyBase64 = "yourtokensigningkey"
            };

            var swt = validator.ValidateToken(token);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
