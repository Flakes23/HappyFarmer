using HappyFarmer.AuthService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HappyFarmer.AuthService.Api.Controllers;

[ApiController]
[Route(".well-known")]
public class WellKnownController(RsaKeyProvider keyProvider) : ControllerBase
{
    [HttpGet("jwks.json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetJwks()
    {
        var parameters = keyProvider.Rsa.ExportParameters(includePrivateParameters: false);

        var jwk = new
        {
            kty = "RSA",
            use = "sig",
            alg = "RS256",
            kid = keyProvider.Kid,
            n = Base64UrlEncode(parameters.Modulus!),
            e = Base64UrlEncode(parameters.Exponent!),
        };

        return Ok(new { keys = new[] { jwk } });
    }

    private static string Base64UrlEncode(byte[] input) =>
        Convert.ToBase64String(input).Replace('+', '-').Replace('/', '_').TrimEnd('=');
}