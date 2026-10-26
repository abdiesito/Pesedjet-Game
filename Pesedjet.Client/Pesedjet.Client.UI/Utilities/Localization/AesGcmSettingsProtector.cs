using System;
using System.Security.Cryptography;
using System.Text;

namespace Pesedjet.Client.UI.Utilities.Localization;

public sealed class AesGcmSettingsProtector : ISettingsProtector
{
    private const int NonceSizeInBytes = 12;
    private const int TagSizeInBytes = 16;
    private const int KeySizeInBytes = 32;
    private const string KeyMaterialSeparator = "|";
    private const string AuthenticatedContextLabel = "Pesedjet.Client.UserSettings.v1";
    private const string KeyDerivationInfoLabel = "settings-key";

    private static readonly byte[] AuthenticatedContext = Encoding.UTF8.GetBytes(AuthenticatedContextLabel);
    private static readonly byte[] KeyDerivationInfo = Encoding.UTF8.GetBytes(KeyDerivationInfoLabel);

    private readonly byte[] _encryptionKey = DeriveEncryptionKey();

    public byte[] Protect(byte[] plainData)
    {
        var protectedPayload = new byte[NonceSizeInBytes + TagSizeInBytes + plainData.Length];
        Span<byte> nonce = protectedPayload.AsSpan(0, NonceSizeInBytes);
        Span<byte> authenticationTag = protectedPayload.AsSpan(NonceSizeInBytes, TagSizeInBytes);
        Span<byte> cipherText = protectedPayload.AsSpan(NonceSizeInBytes + TagSizeInBytes);

        RandomNumberGenerator.Fill(nonce);
        using var aes = new AesGcm(_encryptionKey, TagSizeInBytes);
        aes.Encrypt(nonce, plainData, cipherText, authenticationTag, AuthenticatedContext);

        return protectedPayload;
    }

    public bool TryUnprotect(byte[] protectedData, out byte[] plainData)
    {
        bool isUnprotected = false;
        plainData = [];

        if (protectedData.Length >= NonceSizeInBytes + TagSizeInBytes)
        {
            try
            {
                Span<byte> payload = protectedData.AsSpan();
                Span<byte> nonce = payload[..NonceSizeInBytes];
                Span<byte> authenticationTag = payload.Slice(NonceSizeInBytes, TagSizeInBytes);
                Span<byte> cipherText = payload[(NonceSizeInBytes + TagSizeInBytes)..];
                var decryptedData = new byte[cipherText.Length];

                using var aes = new AesGcm(_encryptionKey, TagSizeInBytes);
                aes.Decrypt(nonce, cipherText, authenticationTag, decryptedData, AuthenticatedContext);
                plainData = decryptedData;
                isUnprotected = true;
            }
            catch (CryptographicException)
            {
                isUnprotected = false;
            }
        }

        return isUnprotected;
    }

    private static byte[] DeriveEncryptionKey()
    {
        byte[] keyMaterial = Encoding.UTF8.GetBytes(
            $"{Environment.UserName}{KeyMaterialSeparator}{Environment.MachineName}");

        return HKDF.DeriveKey(
            HashAlgorithmName.SHA256, keyMaterial, KeySizeInBytes, salt: AuthenticatedContext, info: KeyDerivationInfo);
    }
}
