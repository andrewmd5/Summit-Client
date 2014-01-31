using System;
using System.IO;
using System.Security.Cryptography;

namespace CodeUSAClient
{
    public class Encryption
    {
        private static byte[] DESInitializationVector = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
        private static byte[] DESKey = {200, 5, 0x4e, 0xe8, 9, 6, 0, 4};
        public static Encryption INSTANCE = new Encryption();

        public static string Decrypt(string value)
        {
            string str;
            try
            {
                using (var provider = new DESCryptoServiceProvider())
                {
                    using (var stream = new MemoryStream(Convert.FromBase64String(value)))
                    {
                        using (
                            var stream2 = new CryptoStream(stream,
                                provider.CreateDecryptor(DESKey, DESInitializationVector), CryptoStreamMode.Read))
                        {
                            using (var reader = new StreamReader(stream2))
                            {
                                str = reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                str = value;
            }
            catch (FormatException)
            {
                str = value;
            }
            return str;
        }

        public static string Encrypt(string value)
        {
            string str;
            using (var provider = new DESCryptoServiceProvider())
            {
                using (var stream = new MemoryStream())
                {
                    using (
                        var stream2 = new CryptoStream(stream, provider.CreateEncryptor(DESKey, DESInitializationVector),
                            CryptoStreamMode.Write))
                    {
                        using (var writer = new StreamWriter(stream2))
                        {
                            writer.Write(value);
                            writer.Flush();
                            stream2.FlushFinalBlock();
                            writer.Flush();
                            str = Convert.ToBase64String(stream.GetBuffer(), 0, (int) stream.Length);
                        }
                    }
                }
            }
            return str;
        }
    }
}