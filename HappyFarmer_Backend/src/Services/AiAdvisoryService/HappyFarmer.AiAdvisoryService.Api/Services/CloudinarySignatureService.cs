using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace HappyFarmer.AiAdvisoryService.Api.Services;

public record UploadSignatureResponse(string Signature, long Timestamp, string ApiKey, string CloudName, string Folder);

/// <summary>
/// Sinh chữ ký cho signed upload thẳng từ frontend lên Cloudinary — API Secret chỉ nằm ở
/// backend, không bao giờ gửi ra ngoài. Cùng pattern với Marketplace/Auth Service
/// (CloudinarySignatureService.cs) — mỗi service tự có bản riêng, không dùng chung.
/// </summary>
public class CloudinarySignatureService(IOptions<CloudinaryOptions> options)
{
    public UploadSignatureResponse GenerateUploadSignature()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var folder = options.Value.Folder;

        var paramsToSign = $"folder={folder}&timestamp={timestamp}";
        var signature = Sha1Hex(paramsToSign + options.Value.ApiSecret);

        return new UploadSignatureResponse(signature, timestamp, options.Value.ApiKey, options.Value.CloudName, folder);
    }

    private static string Sha1Hex(string input)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }
}
