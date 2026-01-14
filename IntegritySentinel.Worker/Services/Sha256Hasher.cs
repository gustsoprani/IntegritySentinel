using IntegritySentinel.Worker.Domain.Interfaces;
using System.Security.Cryptography;
using Polly;
using Polly.Retry;
using System.Text;

namespace IntegritySentinel.Worker.Services;

public class Sha256Hasher : IFileHasher
{
    private readonly AsyncRetryPolicy _retryPolicy;
    public Sha256Hasher()
    {
        _retryPolicy = Policy
            .Handle<IOException>() // Só se o erro for de Arquivo Bloqueado
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromMilliseconds(500 * Math.Pow(2, retryAttempt - 1)));
    }
    public async Task<string?> HashAsync(string filePath)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                using var stream = File.OpenRead(filePath);
                byte[] hashBytes = await SHA256.HashDataAsync(stream);
                return Convert.ToHexString(hashBytes);
            });
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (IOException)
        {
            return null; // Se falhou 3 vezes (Polly desistiu), retorna nulo
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }
}