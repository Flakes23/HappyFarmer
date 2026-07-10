namespace HappyFarmer.AuthService.Api.Services;

public class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";

    public required string CloudName { get; set; }
    public required string ApiKey { get; set; }
    public required string ApiSecret { get; set; }
    public string Folder { get; set; } = "happyfarmer/avatars";
}
