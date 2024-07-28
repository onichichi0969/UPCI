using UPCI.DAL.Models;
using System.Security.Cryptography;
using System.Text;

namespace UPCI.Portal.Helpers
{
    public class Utility
    {
        public static string EncodeString(string value, string key)
        {
            TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
            MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
            byte[] array2 = (((SymmetricAlgorithm)(object)tripleDESCryptoServiceProvider).Key = ((HashAlgorithm)(object)mD5CryptoServiceProvider).ComputeHash(Encoding.UTF8.GetBytes(key)));
            ((SymmetricAlgorithm)(object)tripleDESCryptoServiceProvider).Mode = CipherMode.ECB;
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            string text = Convert.ToBase64String(((SymmetricAlgorithm)(object)tripleDESCryptoServiceProvider).CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length));
            return text.Replace("=", "-").Replace("+", "_").Replace("/", "*");
        }

        public static string DecodeString(string value, string key)
        {
            TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
            MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
            byte[] array2 = (((SymmetricAlgorithm)(object)tripleDESCryptoServiceProvider).Key = ((HashAlgorithm)(object)mD5CryptoServiceProvider).ComputeHash(Encoding.UTF8.GetBytes(key)));
            ((SymmetricAlgorithm)(object)tripleDESCryptoServiceProvider).Mode = CipherMode.ECB;
            byte[] array3 = Convert.FromBase64String(value.Replace("-", "=").Replace("_", "+").Replace("*", "/"));
            return Encoding.UTF8.GetString(((SymmetricAlgorithm)(object)tripleDESCryptoServiceProvider).CreateDecryptor().TransformFinalBlock(array3, 0, array3.Length));
        }

        public static string RandomString(int length)
        {
            Random random = new Random();
            return new string((from s in Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz@", length)
                               select s[random.Next(s.Length)]).ToArray());
        }
        public static int GetInteger(object value)
        {
            return (value != DBNull.Value) ? ((int)value) : 0;
        }

        public static decimal GetDecimal(object value)
        {
            return (value == DBNull.Value) ? 0m : ((decimal)value);
        }

        public static bool GetBool(object value)
        {
            return value != DBNull.Value && (bool)value;
        }

        public static string GetString(object value)
        {
            return (value == DBNull.Value) ? "" : value.ToString();
        }

        public static DateTime GetDateTime(object value)
        {
            return (value == DBNull.Value) ? default(DateTime) : ((DateTime)value);
        }
        
    }
}
