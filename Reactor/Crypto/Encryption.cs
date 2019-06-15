using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Reactor.Crypto
{

    /// <summary>
    /// Encryption helper class. Used to provide functionality for
    /// secure key exchange and encryption.
    /// </summary>
    public class Encryption : IDisposable
    {

        private Aes aes = null;
        private ECDiffieHellmanCng diffieHellman = null;

        private readonly byte[] publicKey;

        /// <summary>
        /// Constructor for a new encryption provider
        /// </summary>
        public Encryption()
        {
            this.aes = new AesCryptoServiceProvider();
            this.diffieHellman = new ECDiffieHellmanCng
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.Sha256
            };

            // This is the key we give the other client
            this.publicKey = this.diffieHellman.PublicKey.ToByteArray();
        }

        /// <summary>
        /// Public key getter
        /// </summary>
        public byte[] PublicKey
        {
            get { return this.publicKey; }
        }

        /// <summary>
        /// Initialization vector getter
        /// </summary>
        public byte[] IV
        {
            get { return this.aes.IV; }
        }

        /// <summary>
        /// Encrypt byte with the public key
        /// </summary>
        /// <param name="publicKey">PK</param>
        /// <param name="toEncrypt">Bytes to encrypt</param>
        /// <returns></returns>
        public byte[] Encrypt(byte[] publicKey, byte[] toEncrypt)
        {
            byte[] encryptedMessage;
            var key = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob);
            var derivedKey = this.diffieHellman.DeriveKeyMaterial(key); // "Common secret"

            this.aes.Key = derivedKey;

            using (var cipherText = new MemoryStream())
            {
                using (var encryptor = this.aes.CreateEncryptor())
                {
                    using (var cryptoStream = new CryptoStream(cipherText, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] ciphertextMessage = toEncrypt;
                        cryptoStream.Write(ciphertextMessage, 0, ciphertextMessage.Length);
                    }
                }

                encryptedMessage = cipherText.ToArray();
            }

            return encryptedMessage;
        }

        /// <summary>
        /// Decrypt using the derived encryption key
        /// </summary>
        /// <param name="publicKey">PK</param>
        /// <param name="encryptedMessage">Data</param>
        /// <param name="iv">IV</param>
        /// <returns>Decrypted bytes</returns>
        public byte[] Decrypt(byte[] publicKey, byte[] encryptedMessage, byte[] iv)
        {
            byte[] decryptedMessage;
            var key = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob);
            var derivedKey = this.diffieHellman.DeriveKeyMaterial(key);

            this.aes.Key = derivedKey;
            this.aes.IV = iv;

            using (var plainText = new MemoryStream())
            {
                using (var decryptor = this.aes.CreateDecryptor())
                {
                    using (var cryptoStream = new CryptoStream(plainText, decryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(encryptedMessage, 0, encryptedMessage.Length);
                    }
                }

                decryptedMessage = plainText.ToArray();
            }

            return decryptedMessage;
        }


        #region IDisposable Members

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposable Member
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.aes != null)
                    this.aes.Dispose();

                if (this.diffieHellman != null)
                    this.diffieHellman.Dispose();
            }
        }
        #endregion


        #region AES Encryption

        /// <summary>
        /// Encrypt data using AES256
        /// </summary>
        /// <param name="bytesToBeEncrypted">Data</param>
        /// <param name="passwordBytes">Key</param>
        /// <param name="saltBytes">Salt</param>
        /// <returns>Encrypted bytes</returns>
        public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes, byte[] saltBytes)
        {
            byte[] encryptedBytes = null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        /// <summary>
        /// Decrypt using AES256
        /// </summary>
        /// <param name="bytesToBeDecrypted">Data</param>
        /// <param name="passwordBytes">Key</param>
        /// <param name="saltBytes">Salt</param>
        /// <returns>Decrypted bytes</returns>
        public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes, byte[] saltBytes)
        {
            byte[] decryptedBytes = null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        #endregion


    }
}
