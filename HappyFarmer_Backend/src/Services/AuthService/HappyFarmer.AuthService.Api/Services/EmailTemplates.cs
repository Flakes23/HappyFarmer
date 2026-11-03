using System.Net;

namespace HappyFarmer.AuthService.Api.Services;

/// <summary>
/// Template HTML cho email gửi người dùng. Dùng table + inline style vì hầu hết email client
/// (Gmail, Outlook) không hỗ trợ CSS ngoài hay flex/grid. Màu theo thương hiệu: xanh #2f855a, nền kem #fffbeb.
/// </summary>
public static class EmailTemplates
{
    public static string PasswordResetOtp(string fullName, string otp, int validMinutes)
    {
        var name = WebUtility.HtmlEncode(fullName);
        var digits = string.Join("", otp.Select(c =>
            $"<td style=\"padding:0 4px;\"><div style=\"width:44px;height:56px;line-height:56px;text-align:center;" +
            $"font-size:28px;font-weight:700;color:#2f855a;background:#f0fff4;border:1px solid #c6f6d5;border-radius:8px;\">{c}</div></td>"));

        return $$"""
        <!DOCTYPE html>
        <html lang="vi">
        <head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;padding:0;background:#fffbeb;font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#fffbeb;padding:32px 12px;">
            <tr><td align="center">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:520px;background:#ffffff;border:1px solid #e2dcc8;border-radius:12px;overflow:hidden;">
                <tr><td style="background:#2f855a;padding:24px 32px;">
                  <span style="font-size:22px;font-weight:700;color:#ffffff;letter-spacing:.3px;">🌱 HappyFarmer</span>
                </td></tr>
                <tr><td style="padding:32px;">
                  <h1 style="margin:0 0 12px;font-size:20px;color:#1a202c;">Đặt lại mật khẩu</h1>
                  <p style="margin:0 0 20px;font-size:15px;line-height:1.6;color:#4a5568;">
                    Xin chào <b>{{name}}</b>,<br>
                    Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Hãy nhập mã xác nhận bên dưới vào trang web để tiếp tục:
                  </p>
                  <table role="presentation" align="center" cellpadding="0" cellspacing="0" style="margin:24px auto;"><tr>{{digits}}</tr></table>
                  <p style="margin:0 0 8px;text-align:center;font-size:14px;color:#718096;">
                    Mã có hiệu lực trong <b style="color:#dd6b20;">{{validMinutes}} phút</b>.
                  </p>
                  <hr style="border:none;border-top:1px solid #e2dcc8;margin:28px 0 20px;">
                  <p style="margin:0;font-size:13px;line-height:1.6;color:#718096;">
                    🔒 Vì lý do bảo mật, tuyệt đối không chia sẻ mã này cho bất kỳ ai, kể cả người tự xưng là nhân viên HappyFarmer.<br>
                    Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này — mật khẩu của bạn vẫn an toàn.
                  </p>
                </td></tr>
                <tr><td style="background:#f3ead3;padding:16px 32px;text-align:center;font-size:12px;color:#718096;">
                  © HappyFarmer — Đồng hành cùng nông dân Việt
                </td></tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
    }
}
