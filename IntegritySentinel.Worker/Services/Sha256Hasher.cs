using IntegritySentinel.Worker.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace IntegritySentinel.Worker.Services;

public class Sha256Hasher : IFileHasher
{
    public async Task<string?> HashAsync(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            byte[] hashBytes = await SHA256.HashDataAsync(stream);
            return Convert.ToHexString(hashBytes);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }
}