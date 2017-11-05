using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Protega___AES_File_Converter.Classes
{
    static class AES_Converter
    {

        public static string Decrypt(string sKey, string sIV, string sData)
        {
            byte[] cipherText = Encoding.Default.GetBytes(sData);
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");

            byte[] Key = Encoding.ASCII.GetBytes(sKey);
            byte[] IV = Encoding.ASCII.GetBytes(sIV);

            // Declare the string used to hold the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd().Replace("\0","");
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
