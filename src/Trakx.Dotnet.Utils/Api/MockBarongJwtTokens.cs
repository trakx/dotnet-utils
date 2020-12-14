using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Trakx.Dotnet.Utils.Api
{
    public class MockBarongJwtTokens
    {
        private static readonly Assembly Assembly = typeof(MockBarongJwtTokens).Assembly;
        private static readonly string Namespace = typeof(MockBarongJwtTokens).Namespace ?? string.Empty;

        public static string Issuer { get; } = "barong";

        public MockBarongJwtTokens()
        {
            var rsa = RSA.Create();
            var stream = Assembly.GetManifestResourceStream($"{Namespace}.privBarongMockKey.txt");
            using var reader = new StreamReader(stream!, Encoding.UTF8);
            var lines = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line != null) lines.Add(line);
            }
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(string.Join("", lines)), out _);
            var securityKey = new RsaSecurityKey(rsa);
            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
        }


        public static readonly IEnumerable<Claim> TrakxClaims = new List<Claim>
        {
            new Claim("iat", $"{DateTime.UtcNow}"), 
            new Claim("sub", "session"), 
            new Claim("jti", Guid.NewGuid().ToString()), 
            new Claim("uid", Guid.NewGuid().ToString()), 
            new Claim("email", "dupond@trakx.io"), 
            new Claim("role", "Admin"), 
            new Claim("level", "3"), 
            new Claim("state", "active"), 
            new Claim("referral_id", "null")
        };

        private readonly SigningCredentials _signingCredentials;

        public string GenerateJwtToken(IEnumerable<Claim>? claims = default)
        {
            var token = new JwtSecurityToken(Issuer, "peatio, barong", 
                claims ?? TrakxClaims, null, DateTime.UtcNow.AddMinutes(20), _signingCredentials);

            var sTokenHandler = new JwtSecurityTokenHandler();
            return sTokenHandler.WriteToken(token);
        }
    }
}
