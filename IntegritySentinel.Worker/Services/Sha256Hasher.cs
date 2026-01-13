using IntegritySentinel.Worker.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace IntegritySentinel.Worker.Services;

public class Sha256Hasher : IFileHasher
{
    public async Task<string?> HashAsync(string filePath)
    {
        const int MaxTentativas = 3;
        const int DelayMs = 500;
        for (int i = 1; i <= MaxTentativas; i++)
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
            catch (IOException)
            {
                if (i == MaxTentativas) return null;
                await Task.Delay(DelayMs);
            } 
        }
        return null;
    }
}