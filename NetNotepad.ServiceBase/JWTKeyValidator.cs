using System.Security.Cryptography;
using System.Text;

namespace NetNotepad.ServiceBase
{
    public static class JWTKeyValidator
    {
        public static bool ValidatePems(byte[] privatePem, byte[] publicPem)
        {
#pragma warning disable
            using MLDsa privatePemMLDsa = MLDsa.ImportFromPem(Encoding.ASCII.GetString(privatePem));
            using MLDsa publicPemMLDsa = MLDsa.ImportFromPem(Encoding.ASCII.GetString(publicPem));
#pragma warning restore

            byte[] data = Guid.NewGuid().ToByteArray();
            byte[] sign = privatePemMLDsa.SignData(data);
            return publicPemMLDsa.VerifyData(data, sign);
        }
    }
}