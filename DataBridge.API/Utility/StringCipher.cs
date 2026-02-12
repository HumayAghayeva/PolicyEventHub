using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace DataBridge.API.Utility
{
    public static class StringCipher
    {
        public static string TripleDesDecrypt(this string text, string? encryptionKey = null, bool hexMode = false)
        {
            byte[] keyArray;
            byte[] toEncryptArray = hexMode ? Convert.FromHexString(text) : Convert.FromBase64String(text);
            string key = encryptionKey ?? Key;

            keyArray = Encoding.UTF8.GetBytes(key);

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();

            return Encoding.UTF8.GetString(resultArray);
        }
        private static string Key
        {
            get
            {
                return "4qBfOkPGebwSNMZzZw1DKaxH";
            }
        }
    }
}
