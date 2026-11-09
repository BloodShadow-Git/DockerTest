using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NetNotepad.Contracts;

namespace NetNotepad.ServiceBase
{
    public class JWTValidator
    {
        private byte[]? _publicPem;

        public bool Available => _publicPem != null;

        public JWTValidator SetPublicPem(byte[]? publicPem)
        {
            if (publicPem == null) { throw new Exception("Empty private pem"); }
            _publicPem = publicPem;
            return this;
        }

        public bool Validate(string token, out ClaimsPrincipal principal)
        {
            if (_publicPem == null) { throw new Exception("Empty public key"); }
            if (string.IsNullOrEmpty(token)) { throw new Exception("Empty token"); }
            principal = null!;

            try
            {
                AsymmetricSecurityKey key = EncryptionManager.CreateKey(_publicPem);
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

                    ValidAlgorithms = [EncryptionManager.Algorithm]
                };

                JwtSecurityTokenHandler handler = new();
                principal = handler.ValidateToken(token, parameters, out SecurityToken secToken);
                ValidateClaims(principal);
                return true;
            }
            catch { return false; }
        }

        private static void ValidateClaims(ClaimsPrincipal principal)
        {
            string? sub = principal.FindFirst(JWTNames.SUB)?.Value;
            if (string.IsNullOrEmpty(sub)) { throw new SecurityTokenException("Missing subject"); }
            if (!Guid.TryParse(sub, out _)) { throw new SecurityTokenException("Invalid user guid"); }
            string? jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti)) { throw new SecurityTokenException("Missing jti"); }
        }
    }
}