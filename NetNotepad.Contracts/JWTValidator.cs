using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace NetNotepad.Contracts
{
    public class JWTValidator
    {
        private string? _publicPem;

        public bool Available => !string.IsNullOrEmpty(_publicPem);

        public JWTValidator SetPublicPem(string? publicPem)
        {
            if (string.IsNullOrEmpty(publicPem)) { throw new Exception("Empty private pem"); }
            _publicPem = publicPem;
            return this;
        }

        public ClaimsPrincipal Validate(string token)
        {
            if (string.IsNullOrEmpty(_publicPem)) { throw new Exception("Empty public key"); }
            if (string.IsNullOrEmpty(token)) { throw new Exception("Empty token"); }

            RSA rsa = RSA.Create();
            rsa.ImportFromPem(_publicPem);
            RsaSecurityKey key = new(rsa);
            TokenValidationParameters parameters = new()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,

                ValidateIssuer = true,
                ValidIssuer = JWTNames.ISS,

                ValidateAudience = true,
                ValidAudience = JWTNames.AUD,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero,

                ValidAlgorithms = [SecurityAlgorithms.RsaSha256]
            };

            JwtSecurityTokenHandler handler = new();
            ClaimsPrincipal principal = handler.ValidateToken(token, parameters, out SecurityToken secToken);
            ValidateClaims(principal);
            return principal;
        }

        private static void ValidateClaims(ClaimsPrincipal principal)
        {
            string? sub =
                principal.FindFirst(JWTNames.SUB)?.Value;

            if (string.IsNullOrEmpty(sub))
                throw new SecurityTokenException(
                    "Missing subject");


            if (!Guid.TryParse(sub, out _))
                throw new SecurityTokenException(
                    "Invalid user guid");


            string? jti =
                principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(jti))
                throw new SecurityTokenException(
                    "Missing jti");
        }
    }
}