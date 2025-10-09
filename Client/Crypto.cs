using System;
using System.Security.Cryptography;
using System.Text;

namespace kTVCSSBlazor.Client;

public class Crypto
{
    public static string Encrypt(string plainText)
    {
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes("vJWXaXaVUSpj6bycLYblJot3EjwM2bjE");
            aes.GenerateIV();

            using (var encryptor = aes.CreateEncryptor())
            {
                byte[] encryptedBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);
                byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];

                Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                return Convert.ToBase64String(result);
            }
        }
    }

}
