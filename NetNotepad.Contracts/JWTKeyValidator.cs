using System.Security.Cryptography;

namespace NetNotepad.Contracts
{
    public static class JWTKeyValidator
    {
        public static bool ValidatePems(string privatePem, string publicPem)
        {
            using RSA privatePemRSA = RSA.Create();
            privatePemRSA.ImportFromPem(privatePem);

            using RSA publicPemRSA = RSA.Create();
            publicPemRSA.ImportFromPem(publicPem);

            byte[] data = Guid.NewGuid().ToByteArray();
            byte[] sign = privatePemRSA.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return publicPemRSA.VerifyData(data, sign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}