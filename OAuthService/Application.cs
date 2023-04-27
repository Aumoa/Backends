// Copyright 2020-2022 Aumoa.lib. All right reserved.

using AspNETUtilities;

using OAuthService.Controllers;
using OAuthService.Options;
using OAuthService.Services;

namespace OAuthService;

public class Application : EntryPoint
{
    public Application(string[] Args) : base(Args)
    {
    }

    public override void ConfigureServices(IServiceCollection Services, IConfiguration Configuration)
    {
        Services.Configure<DbAccountsOption>(Configuration.GetSection("DbAccounts"));

        Services.AddHostedService<MigrationWorker>();
    }
}