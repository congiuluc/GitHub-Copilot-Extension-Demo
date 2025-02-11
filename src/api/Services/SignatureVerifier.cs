using System;
using System.Security.Cryptography;
using System.Text;
using System.Security.Cryptography.X509Certificates;

public static class SignatureVerifier
{
    public static bool VerifyRequest(string rawBody, string signature, string key)
    {
        if (string.IsNullOrEmpty(rawBody)) throw new ArgumentException("Invalid payload");
        if (string.IsNullOrEmpty(signature)) throw new ArgumentException("Invalid signature");
        if (string.IsNullOrEmpty(key)) throw new ArgumentException("Invalid key");

        try
        {
            using var publicKey = ECDsa.Create();

            byte[] dataBytes = Encoding.UTF8.GetBytes(rawBody);

            byte[] signatureBytes = Convert.FromBase64String(signature);

            var trimmedKey = key.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\n", "");
            publicKey.ImportSubjectPublicKeyInfo(Convert.FromBase64String(trimmedKey), out _);

            return publicKey.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
        }
        catch
        {
            return false;
        }
    }

}