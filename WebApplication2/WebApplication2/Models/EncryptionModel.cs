using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YourNamespace.Models
{
    public class EncryptionModel
    {
        private readonly int _keySize;
        public EncryptionModel(int keySize)
        {
            _keySize = keySize;
        }

        // AES anahtarı oluşturma
        public byte[] GenerateAESKey()
        {
            using (var aes = new AesCryptoServiceProvider { KeySize = _keySize })
            {
                aes.GenerateKey();
                return aes.Key;
            }
        }

        // Şifreleme işlemi
        public byte[] EncryptData(byte[] data, byte[] key)
        {
            using (var aes = new AesCryptoServiceProvider { KeySize = _keySize, Key = key, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 })
            {
                aes.GenerateIV();
                var iv = aes.IV;
                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        // Çözme işlemi
        public byte[] DecryptData(byte[] encryptedData, byte[] key)
        {
            using (var aes = new AesCryptoServiceProvider { KeySize = _keySize, Key = key, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 })
            {
                using (var ms = new MemoryStream(encryptedData))
                {
                    var iv = new byte[16];
                    ms.Read(iv, 0, iv.Length);
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var decryptedStream = new MemoryStream())
                    {
                        cs.CopyTo(decryptedStream);
                        return decryptedStream.ToArray();
                    }
                }
            }
        }
    }
}
