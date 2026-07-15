namespace HappyFarmer.AiAdvisoryService.Api.Entities;

/// <summary>
/// Bảng override tùy chọn: chỉ chứa số liệu nông học đã kiểm chứng cho vài cây quan trọng nhất
/// (hiện tại chỉ Lúa). Cây không có trong bảng này vẫn được dự đoán bình thường — Gemini tự dùng
/// kiến thức nông học chung, không bị chặn bởi danh sách này.
/// </summary>
public class CropProfile
{
    public int Id { get; set; }
    public required string CropTypeCode { get; set; }
    public required string CropNameVi { get; set; }
    public int AvgDaysToHarvest { get; set; }
    public double IdealTempMin { get; set; }
    public double IdealTempMax { get; set; }
    public double IdealRainfallMm { get; set; }
    public string? Notes { get; set; }
}
