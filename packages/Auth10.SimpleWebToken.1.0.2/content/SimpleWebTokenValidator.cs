/* 
 * Credits: http://zamd.net/2011/02/08/using-simple-web-token-swt-with-wif/ 
 *          http://netfx.codeplex.com/
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.Security.Cryptography;

namespace Auth10.Swt
{
    /// <summary>
    /// Validates SimpleWebTokens
    /// </summary>
    /// <example>
    /// <code>
    /// var validator = new SimpleWebTokenValidator();
    /// validator.SharedKeyBase64 = "yoursharedkeyinbase64";
    /// var swt = validator.ValidateToken(token); // this throws if it's invalid
    /// // access the claims doing swt.Claims (Dictionary)
    /// </code>
    /// </example>
    public class SimpleWebTokenValidator 
    {
        public SimpleWebTokenValidator()
        {
            this.AllowedAudiences = new List<Uri>();
        }

        /// <summary>
        /// Symmetric key in base64 format
        /// </summary>
        public string SharedKeyBase64 { get; set; }

        /// <summary>
        /// Allowed audience URIs (optional)
        /// </summary>
        public List<Uri> AllowedAudiences { get; set; }

        /// <summary>
        /// Allowed issuer (optional)
        /// </summary>
        public string AllowedIssuer { get; set; }

        public SimpleWebToken ValidateTokenFromBase64(string base64BinaryToken)
        {
            if (base64BinaryToken == null)
                throw new HttpException((int)System.Net.HttpStatusCode.Unauthorized, "SWT not found");

            var swtBuffer = Convert.FromBase64String(base64BinaryToken);
            var swt = Encoding.Default.GetString(swtBuffer);

            return ValidateToken(swt);
        }

        public SimpleWebToken ValidateToken(string token)
        {
            if (token == null)
                throw new HttpException((int)System.Net.HttpStatusCode.Unauthorized, "SWT not found");

            var swt = new SimpleWebToken(token);
            var securityKey = Convert.FromBase64String(this.SharedKeyBase64);

            if (securityKey == null)
                throw new HttpException((int)System.Net.HttpStatusCode.Unauthorized, "Missing shared key");

            if (!IsHMACValid(swt.RawToken, securityKey))
                throw new HttpException((int)System.Net.HttpStatusCode.Unauthorized, "Invalid signature");

            if (swt.IsExpired)
                throw new HttpException((int)System.Net.HttpStatusCode.Unauthorized, "Token expired");

            if (this.AllowedAudiences != null && this.AllowedAudiences.Count > 0)
            {
                var swtAudienceUri = default(Uri);
                if (!Uri.TryCreate(swt.Audience, UriKind.RelativeOrAbsolute, out swtAudienceUri))
                    throw new HttpException((int)System.Net.HttpStatusCode.Unauthorized, "Invalid audience");

                if (!this.AllowedAudiences.Any(uri => uri == swtAudienceUri))
                    throw new HttpException((int)System.Net.HttpStatusCode.Unauthorized, "Audience not found");
            }

            if (!string.IsNullOrEmpty(this.AllowedIssuer))
            {
                if (!this.AllowedIssuer.Equals(swt.Issuer, StringComparison.Ordinal))
                {
                    throw new HttpException((int)System.Net.HttpStatusCode.Unauthorized, "Invalid issuer");
                }
            }

            return swt;
        }

        private static bool IsHMACValid(string swt, byte[] sha256HMACKey)
        {
            var swtWithSignature = swt.Split(new string[] { String.Format("&{0}=", SwtConstants.HmacSha256) }, StringSplitOptions.None);
            if (swtWithSignature.Length != 2)
                return false;

            using (var hmac = new HMACSHA256(sha256HMACKey))
            {
                var locallyGeneratedSignatureInBytes = hmac.ComputeHash(Encoding.ASCII.GetBytes(swtWithSignature[0]));
                var locallyGeneratedSignature = HttpUtility.UrlEncode(Convert.ToBase64String(locallyGeneratedSignatureInBytes));

                return String.Equals(locallyGeneratedSignature, swtWithSignature[1], StringComparison.InvariantCulture);
            }
        }
    }

    public class SimpleWebToken
    {
        private DateTime validFrom = DateTime.UtcNow;

        public SimpleWebToken(string rawToken)
        {
            this.RawToken = rawToken;
            this.Parse();
        }

        public bool IsExpired
        {
            get
            {
                var expiresOn = this.ExpiresOn.ToEpochTime();
                var currentTime = DateTime.UtcNow.ToEpochTime();

                return currentTime > expiresOn;
            }
        }

        public override string ToString()
        {
            return this.RawToken;
        }

        private void Parse()
        {
            this.Claims = new Dictionary<string, string>();

            foreach (var rawNameValue in this.RawToken.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (rawNameValue.StartsWith(SwtConstants.HmacSha256 + "="))
                    continue;

                var nameValue = rawNameValue.Split('=');

                if (nameValue.Length != 2)
                    throw new InvalidSecurityTokenException(string.Format(
                        "Invalid token contains a name/value pair missing an = character: '{0}'", rawNameValue));

                var key = HttpUtility.UrlDecode(nameValue[0]);

                if (this.Claims.ContainsKey(key))
                    throw new InvalidSecurityTokenException("Duplicated name token.");

                var values = HttpUtility.UrlDecode(nameValue[1]);

                switch (key)
                {
                    case SwtConstants.Audience:
                        this.Audience = values;
                        break;
                    case SwtConstants.ExpiresOn:
                        this.ExpiresOn = ulong.Parse(values).ToDateTimeFromEpoch();
                        break;
                    case SwtConstants.Issuer:
                        this.Issuer = values;
                        break;
                    default:
                        // We may have more than one value in SWT.
                        foreach (var value in values.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            this.Claims.Add(key, value);
                        }
                        break;
                }
            }
        }

        public string Audience { get; private set; }
        public Dictionary<string, string> Claims { get; private set; }
        public DateTime ExpiresOn { get; private set; }
        public string Issuer { get; private set; }
        public string RawToken { get; private set; }
    }

    internal class SwtConstants
    {
        public const string Audience = "Audience";
        public const string Issuer = "Issuer";
        public const string ExpiresOn = "ExpiresOn";
        public const string HmacSha256 = "HMACSHA256";
    }

    class InvalidSecurityTokenException : Exception
    {
        public InvalidSecurityTokenException(string message)
            : base(message)
        {
        }
    }

    #region BSD License
    /* 
        Copyright (c) 2010, NETFx
        All rights reserved.

        Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

        * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

        * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

        * Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

        THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
        */
    #endregion

    internal static class DateTimeEpochExtensions
    {
        /// <summary>
        /// Converts the given date value to epoch time.
        /// </summary>
        public static ulong ToEpochTime(this DateTime dateTime)
        {
            var date = dateTime.ToUniversalTime();
            var ts = date - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            return Convert.ToUInt64(ts.TotalSeconds);
        }

        /// <summary>
        /// Converts the given date value to epoch time.
        /// </summary>
        public static ulong ToEpochTime(this DateTimeOffset dateTime)
        {
            var date = dateTime.ToUniversalTime();
            var ts = date - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

            return Convert.ToUInt64(ts.TotalSeconds);
        }

        /// <summary>
        /// Converts the given epoch time to a <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/> kind.
        /// </summary>
        public static DateTime ToDateTimeFromEpoch(this ulong secondsSince1970)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(secondsSince1970);
        }

        /// <summary>
        /// Converts the given epoch time to a UTC <see cref="DateTimeOffset"/>.
        /// </summary>
        public static DateTimeOffset ToDateTimeOffsetFromEpoch(this ulong secondsSince1970)
        {
            return new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(secondsSince1970);
        }
    }
}
