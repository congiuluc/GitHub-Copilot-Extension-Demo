using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class RequestValidationService
{
    private readonly GitHubService _gitHubService;
    private readonly IMemoryCache _cache;

    public RequestValidationService(GitHubService gitHubService, IMemoryCache cache)
    {
        _gitHubService = gitHubService;
        _cache = cache;
    }

    public async Task<bool> ValidateRequestAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-GitHub-Public-Key-Signature", out var signature) &&
            context.Request.Headers.TryGetValue("X-GitHub-Public-Key-Identifier", out var keyId))
        {
            // Get valid public keys from cache or GitHub
            if (!_cache.TryGetValue("GitHubPublicKeys", out List<PublicKey>? keys) || keys == null)
            {
                keys = await _gitHubService.GetPublicKeysAsync();

                // Set cache options
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

                // Save data in cache
                _cache.Set("GitHubPublicKeys", keys, cacheEntryOptions);
            }

            var key = keys?.FirstOrDefault(k => k.KeyIdentifier == keyId);
            if (key == null)
            {
                return false;
            }

            // Get signed body
            context.Request.EnableBuffering();
            var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
            int bytesRead, totalBytesRead = 0;
            while ((bytesRead = await context.Request.Body.ReadAsync(buffer, totalBytesRead, buffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;
            }
            var body = Encoding.UTF8.GetString(buffer, 0, totalBytesRead);

            context.Request.Body.Position = 0;  // Rewind the stream to 0

            var validSignature = SignatureVerifier.VerifyRequest(body, signature.ToString(), key.Key);
            return validSignature;
        }
        return false;
    }
}