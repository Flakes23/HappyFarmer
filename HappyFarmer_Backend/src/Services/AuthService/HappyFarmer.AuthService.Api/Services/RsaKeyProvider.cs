using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace HappyFarmer.AuthService.Api.Services;

/// <summary>
/// Loads the RS256 signing key from disk, generating a new key pair on first run (dev convenience).
/// The Kid is derived from the public key modulus so it stays stable across restarts without extra storage.
/// </summary>
public class RsaKeyProvider
{
    public RSA Rsa { get; }
    public string Kid { get; }

    public RsaKeyProvider(IOptions<JwtOptions> options, IWebHostEnvironment env)
    {
        var path = Path.IsPathRooted(options.Value.PrivateKeyPath)
            ? options.Value.PrivateKeyPath
            : Path.Combine(env.ContentRootPath, options.Value.PrivateKeyPath);

        Rsa = RSA.Create(2048);

        if (File.Exists(path))
        {
            Rsa.ImportFromPem(File.ReadAllText(path));
        }
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, Rsa.ExportPkcs8PrivateKeyPem());
        }

        Kid = Convert.ToHexString(SHA256.HashData(Rsa.ExportRSAPublicKey()))[..16].ToLowerInvariant();
    }
}