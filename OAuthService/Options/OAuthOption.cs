// Copyright 2020-2022 Aumoa.lib. All right reserved.

namespace OAuthService.Options;

public record OAuthOption
{
    public required int AccessTokenExpires { get; set; }

    public required int RefreshTokenExpires { get; set; }
}