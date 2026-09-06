using System;
using System.Security.Cryptography;
using System.Text;

namespace cdm.Helpers
{
    public static class EncryptionHelper
    {
        // ✅ المفتاح والمتجه الابتدائي (يجب أن يكونا 16 حرفًا بالضبط لأمان AES-128)
        private static readonly string key = "CDM@EncryptKey!!";   // 16-char = 128-bit
        private static readonly string iv = "CDM@VectorInit!!";    // 16-char = 128-bit

        /// <summary>
        /// تشفير نص باستخدام AES ثم إرجاعه كـ Base64
        /// </summary>
        public static string Encrypt(string plainText)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = Encoding.UTF8.GetBytes(iv);

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                        byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// فك تشفير نص مشفر باستخدام AES
        /// </summary>
        public static string Decrypt(string encryptedText)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = Encoding.UTF8.GetBytes(iv);

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
