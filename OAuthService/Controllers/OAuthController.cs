// Copyright 2020-2022 Aumoa.lib. All right reserved.

using System.Data;

using AspNETUtilities.Misc;

using Dapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using MySql.Data.MySqlClient;

using OAuthService.Options;

namespace OAuthService.Controllers;

[ApiController]
[Route("api/oauth")]
public class OAuthController : ControllerBase
{
    private readonly DbAccountsOption Config;
    public OAuthController(IOptions<DbAccountsOption> Config)
    {
        this.Config = Config.Value;
    }

    public record TakeTokenBody
    {
        public required string grant_type { get; set; }

        public required string username { get; set; }

        public required string password { get; set; }

        public required string service { get; set; }

        public required string client_id { get; set; }

        public required string access_type { get; set; }
    }

    [HttpPost("token")]
    public async Task<IActionResult> TakeTokenAsync([FromForm] TakeTokenBody Body)
    {
        const string QUERY = "SELECT COUNT(*) FROM `Account` WHERE `PlatformId` = @username AND `Password` = @SHAPassword";
        using var Conn = NewConnection();

        if (Body.grant_type.Equals("password", StringComparison.OrdinalIgnoreCase))
        {
            int Aff = await Conn.ExecuteScalarAsync<int>(QUERY, new { Body.username, SHAPassword = Global.GenerateSHAPassword(Body.password) });
            if (Aff == 0)
            {
                return Unauthorized();
            }

            return Ok(new
            {
                refresh_token = "1",
                access_token = "2",
                expires_in = 900,
                scope = Body.service
            });
        }
        else
        {
            return BadRequest();
        }
    }

    private IDbConnection NewConnection()
    {
        return new MySqlConnection(Config.ConnectionString);
    }
}