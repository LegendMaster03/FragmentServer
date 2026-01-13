using System;
using System.Buffers.Binary;
using Microsoft.Extensions.Configuration;

namespace Fragment.NetSlum.Networking.Crypto;

public class CryptoHandler
{
    public BlowfishProvider ClientCipher { get; set; }
    public BlowfishProvider ServerCipher { get; internal set; }

    public CryptoHandler(Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        // Read common key from configuration if present. Fallback to BlowfishProvider default.
        var key = configuration["Crypto:CommonKey"];

        if (!string.IsNullOrEmpty(key))
        {
            ClientCipher = new BlowfishProvider(key);
            ClientCipher.Initialize();
        }
        else
        {
            ClientCipher = new BlowfishProvider(); // default ctor initializes with built-in default key
        }

        // Server cipher is left uninitialized until a key exchange occurs
        ServerCipher = new BlowfishProvider();
    }

    public bool TryEncrypt(byte[] data, out byte[] encrypted)
    {
        if (ServerCipher == null || !ServerCipher.Initialized)
        {
            encrypted = data;
            return false;
        }

        encrypted = ServerCipher.Encrypt(data);

        return true;
    }

    public bool TryDecrypt(byte[] encrypted, out byte[] decrypted)
    {
        // If client cipher isn't initialized, decryption will throw; guard accordingly
        if (ClientCipher == null || !ClientCipher.Initialized)
        {
            decrypted = Array.Empty<byte>();
            return false;
        }

        decrypted = ClientCipher.Decrypt(encrypted);

        var receivedChecksum = BinaryPrimitives.ReadUInt16BigEndian(decrypted.AsSpan()[..2]);
        var decryptedChecksum = BlowfishProvider.Checksum(decrypted[2..]);

        decrypted = decrypted[2..];

        return receivedChecksum == decryptedChecksum;
    }
}
