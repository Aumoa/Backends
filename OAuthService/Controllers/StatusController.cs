// Copyright 2020-2022 Aumoa.lib. All right reserved.

using Microsoft.AspNetCore.Mvc;

namespace OAuthService.Controllers;

[ApiController]
[Route("api/status")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}