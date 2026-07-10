using HappyFarmer.AuthService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyFarmer.AuthService.Api.Controllers;

[ApiController]
[Route("api/auth/uploads")]
[Authorize]
public class UploadsController(CloudinarySignatureService signatureService) : ControllerBase
{
    [HttpGet("signature")]
    public ActionResult<UploadSignatureResponse> GetSignature() => Ok(signatureService.GenerateUploadSignature());
}
