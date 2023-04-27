// Copyright 2020-2022 Aumoa.lib. All right reserved.

using System.Diagnostics;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNETUtilities;

public abstract class EntryPoint
{
    public EntryPoint(string[] Args)
    {
        Builder = WebApplication.CreateBuilder(Args);
    }

    public WebApplicationBuilder Builder { get; private init; }

    public WebApplication? App { get; private set; }

    public bool bUseSwagger { get; protected init; } = true;

    public bool bUseHttps { get; protected init; } = true;

    public abstract void ConfigureServices(IServiceCollection Services, IConfiguration Configuration);

    public void Run()
    {
        ConfigureServices(Builder.Services, Builder.Configuration);

        Builder.Services.AddControllers();
        Builder.Services.AddEndpointsApiExplorer();
        Builder.Services.AddSwaggerGen();

        App = Builder.Build();

        if (bUseSwagger)
        {
            App.UseSwagger();
            App.UseSwaggerUI();
        }

        if (bUseHttps)
        {
            App.UseHttpsRedirection();
        }

        App.UseAuthentication();
        App.MapControllers();

        App.Run();
    }
}
