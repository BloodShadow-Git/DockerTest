using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace NetNotepad.Contracts
{
    public class JWTBuilder
    {
        private string? _privatePem;
        private TimeSpan? _lifespan;
        private Guid? _userGuid;

        public bool Available => !string.IsNullOrEmpty(_privatePem) && _lifespan != null && _userGuid != null;

        public JWTBuilder SetPrivatePem(string? privatePem)
        {
            if (string.IsNullOrEmpty(privatePem)) { throw new Exception("Empty private pem"); }
            _privatePem = privatePem;
            return this;
        }

        public JWTBuilder SetLifeSpan(TimeSpan? lifespan)
        {
            _lifespan = lifespan ?? throw new Exception("Invalid life span");
            return this;
        }

        public JWTBuilder SetUserGuid(Guid? userGuid)
        {
            _userGuid = userGuid ?? throw new Exception("Invalid user guid");
            return this;
        }

        public string Build(out Guid jti)
        {
            if (string.IsNullOrEmpty(_privatePem)) { throw new Exception("Secret is invalid"); }
            if (_lifespan == null) { throw new Exception("Life span is invalid"); }
            if (_userGuid == null) { throw new Exception("User GUID is invalid"); }

            DateTime now = DateTime.UtcNow;
            jti = Guid.NewGuid();

            Claim[] claims = [
                new(JWTNames.SUB, ((Guid)_userGuid).ToString()),
                new(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Iss, JWTNames.ISS),
                new(JwtRegisteredClaimNames.Aud, JWTNames.AUD),
                new(JwtRegisteredClaimNames.Jti, jti.ToString())];

            RSA rsa = RSA.Create();
            rsa.ImportFromPem(_privatePem);
            RsaSecurityKey key = new(rsa);
            SigningCredentials credentials = new(key, SecurityAlgorithms.RsaSha256);

            JwtSecurityToken token = new(claims: claims, notBefore: now, expires: now.Add((TimeSpan)_lifespan), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public JWTBuilder Clone()
        {
            JWTBuilder clone = new();
            if (!string.IsNullOrEmpty(_privatePem)) { clone = clone.SetPrivatePem(_privatePem); }
            if (_lifespan != null) { clone = clone.SetLifeSpan(_lifespan); }
            if (_userGuid != null) { clone = clone.SetUserGuid(_userGuid); }
            return clone;
        }
    }
}