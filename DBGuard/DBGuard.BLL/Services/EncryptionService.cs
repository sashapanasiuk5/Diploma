using System.Security.Cryptography;
using System.Text;
using DBGuard.BLL.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace DBGuard.BLL.Services;

public class EncryptionService: IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        string keyString = configuration["Encryption:Key"] 
            ?? throw new InvalidOperationException("Encryption key not found in configuration.");
        
        _key = Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        
        aes.GenerateIV();
        byte[] iv = aes.IV;

        using MemoryStream ms = new();
        using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            cs.Write(plainBytes, 0, plainBytes.Length);
        }

        byte[] encrypted = ms.ToArray();
        
        byte[] result = new byte[iv.Length + encrypted.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        byte[] fullCipher = Convert.FromBase64String(cipherText);

        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        
        byte[] iv = new byte[16];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, 16);
        aes.IV = iv;

        byte[] cipher = new byte[fullCipher.Length - 16];
        Buffer.BlockCopy(fullCipher, 16, cipher, 0, cipher.Length);

        using MemoryStream ms = new();
        using (CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
        {
            cs.Write(cipher, 0, cipher.Length);
        }

        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
