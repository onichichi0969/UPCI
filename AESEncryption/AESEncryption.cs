using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AESEncryption
{
    public sealed class AESEncryption
    {
        private static byte[] GenerateKey(string passphrase)
        {
            // Generate a key based on the passphrase
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(Encoding.UTF8.GetBytes(passphrase));
            }
        }

        private static byte[] GenerateIV()
        {
            return new byte[16]; // Default IV (all zeros)
        }

        public static string Encrypt(string plainText, string passphrase)
        {
            byte[] key = GenerateKey(passphrase);
            byte[] iv = GenerateIV();

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(cryptoStream))
                        {
                            writer.Write(plainText);
                        }
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passphrase)
        {
            byte[] key = GenerateKey(passphrase);
            byte[] iv = GenerateIV();
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cryptoStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
