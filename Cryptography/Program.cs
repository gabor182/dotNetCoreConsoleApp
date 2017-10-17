using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Cryptography
{
    class Program
    {
        public static void Main()
        {
            try
            {

                string original = "Here is some data to encrypt!";

                byte[] compressedData = Compress(original);

                // Create a new instance of the Rijndael
                // class.  This generates a new key and initialization 
                // vector (IV).
                using (Rijndael myRijndael = Rijndael.Create())
                {
                    // Encrypt the compressed data to an array of bytes.
                    byte[] encrypted = Encrypt(compressedData, myRijndael.Key, myRijndael.IV);

                    // Decrypt the bytes to a string.
                    string roundtrip = DecryptAndUnzip(encrypted, myRijndael.Key, myRijndael.IV);

                    //Display the original data and the decrypted data.
                    Console.WriteLine("Original:   {0}", original);
                    Console.WriteLine("Round Trip: {0}", roundtrip);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }

        static byte[] Encrypt(byte[] data, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (Key == null || Key.Length <= 0)
            {
                throw new ArgumentNullException(nameof(Key));
            }

            if (IV == null || IV.Length <= 0)
            {
                throw new ArgumentNullException(nameof(IV));
            }

            byte[] encrypted;
            // Create an Rijndael object
            // with the specified key and IV.
            using (Rijndael rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;
                rijAlg.Padding = PaddingMode.PKCS7;
                //rijAlg.BlockSize = 128;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream stream = new MemoryStream(data))
                {
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            stream.CopyTo(csEncrypt);
                        }

                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        static string DecryptAndUnzip(byte[] cipherData, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherData == null || cipherData.Length <= 0)
            {
                throw new ArgumentNullException(nameof(cipherData));
            }

            if (Key == null || Key.Length <= 0)
            {
                throw new ArgumentNullException(nameof(Key));
            }

            if (IV == null || IV.Length <= 0)
            {
                throw new ArgumentNullException(nameof(IV));
            }

            byte[] contentBytes;

            // Create an Rijndael object
            // with the specified key and IV.
            using (Rijndael rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;
                rijAlg.Padding = PaddingMode.PKCS7;
                //rijAlg.BlockSize = 128;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (var sourceStream = new MemoryStream(cipherData))
                {
                    using (var targetStream = new MemoryStream())
                    {
                        using (var decryptStream = new CryptoStream(sourceStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (var unzipStream = new GZipStream(decryptStream, CompressionMode.Decompress))
                            {
                                unzipStream.CopyTo(targetStream);
                            }
                        }

                        contentBytes = targetStream.ToArray();
                    }
                }

            }

            return Encoding.Default.GetString(contentBytes);
        }

        static byte[] Compress(string message)
        {
            byte[] compressed;

            using (var outStream = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(outStream, CompressionMode.Compress))
                {
                    using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(message)))
                    {
                        memoryStream.CopyTo(compressionStream);
                    }
                }

                compressed = outStream.ToArray();
            }

            return compressed;
        }

        static string Decompress(byte[] compressedData)
        {
            return string.Empty;
        }
    }
}
