// Copyright 2020-2022 Aumoa.lib. All right reserved.

using System.Security.Cryptography;
using System.Text;

namespace AspNETUtilities.Misc;

public static class Global
{
    public static string GenerateSHAPassword(string InPassword)
    {
        using SHA256 Instance = SHA256.Create();
        byte[] HashBytes = Instance.ComputeHash(Encoding.UTF8.GetBytes(InPassword));
        string SHAPassword = string.Empty;
        foreach (var Byte in HashBytes)
        {
            SHAPassword += Byte.ToString("x2");
        }
        return SHAPassword;
    }
}
