using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Byter;
using Microsoft.IdentityModel.Tokens;

namespace Conster.Core;

public static class JWTSolver
{
    private const string CLAIM_KEY = "data";

    public static bool Solve<T>(string token, string key, out T? data)
    {
        data = default;

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(token)) return false;

        try
        {
            new JwtSecurityTokenHandler().ValidateToken
            (
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key.GetBytes()),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                },
                out var tokenResult
            );

            var payload = ((JwtSecurityToken)tokenResult).Claims.First(x => x.Type == CLAIM_KEY).Value;

            data = JsonSerializer.Deserialize<T>(payload) ?? throw new ArgumentNullException(nameof(data));

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"{nameof(JWTSolver)} -> {nameof(Solve)} ({nameof(Exception)}): {e.Message}");
            return false;
        }
    }

    public static string Create<T>(T data, string key, TimeSpan expireAt)
    {
        string payload;

        try
        {
            payload = JsonSerializer.Serialize(data);
        }
        catch
        {
            payload = string.Empty;
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim(CLAIM_KEY, payload)]),
            Expires = DateTime.UtcNow.AddMicroseconds(expireAt.TotalMilliseconds),
            SigningCredentials = new SigningCredentials
            (
                new SymmetricSecurityKey(key.GetBytes()),
                SecurityAlgorithms.HmacSha256
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}