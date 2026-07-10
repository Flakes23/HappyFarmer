using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace HappyFarmer.AuthService.Api.Services;

public record UploadSignatureResponse(string Signature, long Timestamp, string ApiKey, string CloudName, string Folder);

/// <summary>
/// Sinh chữ ký cho signed upload thẳng từ frontend lên Cloudinary — API Secret chỉ nằm ở
/// backend, không bao giờ gửi ra ngoài (khác với unsigned upload preset, dễ bị lạm dụng vì
/// cloud name + preset lộ trong bundle JS).
/// </summary>
public class CloudinarySignatureService(IOptions<CloudinaryOptions> options)
{
    public UploadSignatureResponse GenerateUploadSignature()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var folder = options.Value.Folder;

        // Cloudinary yêu cầu ký các param (trừ file/api_key/cloud_name/resource_type) theo thứ tự
        // alphabet, nối dạng key=value&..., rồi nối trực tiếp API Secret vào cuối trước khi SHA1.
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
