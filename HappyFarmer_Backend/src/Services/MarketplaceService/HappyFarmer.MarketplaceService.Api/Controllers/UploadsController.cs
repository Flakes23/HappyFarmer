using HappyFarmer.MarketplaceService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyFarmer.MarketplaceService.Api.Controllers;

[ApiController]
[Route("api/marketplace/uploads")]
[Authorize(Roles = "Farmer")]
public class UploadsController(CloudinarySignatureService signatureService) : ControllerBase
{
    [HttpGet("signature")]
    public ActionResult<UploadSignatureResponse> GetSignature() => Ok(signatureService.GenerateUploadSignature());
}
