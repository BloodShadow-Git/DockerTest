using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace NetNotepad.ServiceBase
{
    public static class EncryptionManager
    {
        public const string Algorithm = SecurityAlgorithms.MlDsa65;

        public static AsymmetricSecurityKey CreateKey(byte[] pem)
        {
#pragma warning disable
            using MLDsa mLDsa = MLDsa.ImportFromPem(Encoding.ASCII.GetString(pem));
#pragma warning restore
            return new MlDsaSecurityKey(mLDsa);
        }
    }
}