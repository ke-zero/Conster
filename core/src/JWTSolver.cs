using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Conster.Core;

public static class JWTSolver
{
    private const string CLAIM_KEY = "data";
    private static readonly Encoding _encoding = Encoding.UTF8;

    public static bool Solve<T>(string token, string key, out T? data)
    {
        data = default;

        if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(token)) return false;

        var symmetricKey = new SymmetricSecurityKey(_encoding.GetBytes(key));

        try
        {
            new JwtSecurityTokenHandler().ValidateToken
            (
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = symmetricKey,
                    ValidateIssuer = false,
                    ValidateAudience = false
                },
                out var securityToken
            );

            var jwtToken = (JwtSecurityToken)securityToken;
            var payload = jwtToken.Claims.First(x => x.Type == CLAIM_KEY).Value;

            data = JsonSerializer.Deserialize<T>(payload);
            
            if (data == null) throw new ArgumentNullException(nameof(data));

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string Create<T>(T data, string key, TimeSpan expireAt)
    {
        var symmetricKey = new SymmetricSecurityKey(_encoding.GetBytes(key));
        var credential = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

        var payload = JsonSerializer.Serialize(data);
        var claim = new Claim(CLAIM_KEY, payload);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([claim]),
            Expires = DateTime.UtcNow.AddMinutes(expireAt.TotalMinutes),
            SigningCredentials = credential
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}