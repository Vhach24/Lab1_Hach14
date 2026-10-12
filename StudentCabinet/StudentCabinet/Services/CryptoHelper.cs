using System.Security.Cryptography;
using System.Text;

namespace StudentCabinet.Services
{
    public static class CryptoHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("StudentCabinet2026LabFourKey!!!!");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("StudentCabinetIV");

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Key[..32];
            aes.IV = IV[..16];
            using var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(plainText ?? string.Empty);
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;
            using var aes = Aes.Create();
            aes.Key = Key[..32];
            aes.IV = IV[..16];
            using var decryptor = aes.CreateDecryptor();
            var bytes = Convert.FromBase64String(cipherText);
            var decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}