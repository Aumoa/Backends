// Copyright 2020-2022 Aumoa.lib. All right reserved.

using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

using AspNETUtilities.Misc;

using Dapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using MySql.Data.MySqlClient;

namespace OAuthService.Controllers;

[ApiController]
[Route("api/register")]
public class RegisterController : ControllerBase
{
    public record Configuration
    {
        public required string ConnectionString { get; init; }
    }

    private readonly Configuration Config;

    public RegisterController(IOptions<Configuration> Config)
    {
        this.Config = Config.Value;
    }

    [HttpGet("contains")]
    public async Task<IActionResult> GetContainsAsync([FromQuery] string PlatformId)
    {
        using var Conn = NewConnection();

        const string QUERY = "SELECT 1 FROM `Account` WHERE `PlatformId` = @PlatformId";
        var Row = await Conn.QueryFirstOrDefaultAsync(QUERY, new { PlatformId });
        if (Row == null)
        {
            return NotFound();
        }
        else
        {
            return Ok();
        }
    }

    public record CreateAccountBody
    {
        public required string Password { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccountAsync([FromQuery] string PlatformId, [FromBody] CreateAccountBody Body)
    {
        using var Conn = NewConnection();
        const string QUERY = "INSERT INTO `Account`(`PlatformId`, `Password`) VALUES(@PlatformId, @SHAPassword); SELECT LAST_INSERT_ID();";

        try
        {
            ulong Id = await Conn.ExecuteScalarAsync<ulong>(QUERY, new { PlatformId, SHAPassword = Global.GenerateSHAPassword(Body.Password) });
            if (Id == 0)
            {
                throw new InvalidOperationException("Internal error.");
            }

            return Ok(Id);
        }
        catch (MySqlException ME)
        {
            if (ME.Number == (int)MySqlErrorCode.DuplicateKey || ME.Number == (int)MySqlErrorCode.DuplicateKeyEntry)
            {
                return Conflict();
            }
            else
            {
                throw;
            }
        }
    }

    public record UpdateAttributeBody
    {
        public required short Category { get; set; }

        public required JsonNode Content { get; set; }
    }

    [HttpPut("{AccountId}/attributes")]
    public async Task<IActionResult> UpdateAttributeAsync([FromRoute] long AccountId, [FromBody] UpdateAttributeBody Body)
    {
        using var Conn = NewConnection();
        const string QUERY = "INSERT INTO `Attribute`(`Id`, `Category`, `Content`) VALUES(@AccountId, @Category, @Content) ON DUPLICATE KEY UPDATE `Content` = @Content, `UpdateDate` = NOW()";
        int Aff = await Conn.ExecuteAsync(QUERY, new { AccountId, Body.Category, Content = Body.Content.ToJsonString() });
        if (Aff == 0)
        {
            throw new InvalidOperationException("Internal error.");
        }
        return Ok();
    }

    [HttpDelete("{AccountId}/attributes")]
    public async Task<IActionResult> DeleteAttributeAsync([FromRoute] long AccountId, [FromQuery] short Category)
    {
        using var Conn = NewConnection();
        const string QUERY = "DELETE FROM `Attribute` WHERE `AccountId` = @AccountId AND `Category` = @Category";
        int Aff = await Conn.ExecuteAsync(QUERY, new { AccountId, Category });
        return Aff != 0 ? Ok() : NotFound();
    }

    private IDbConnection NewConnection()
    {
        return new MySqlConnection(Config.ConnectionString);
    }
}