using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace Test_Thuyet_Trinh_Oauth2.Manages
{
    public partial class TokenHelper
    {
        private static readonly string Secret = Helpers.GetValueFromAppSetting("SecretToken");

        public static string GenerateToken(string username, string role = "User")
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
            };

            byte[] keyBytes = Encoding.UTF8.GetBytes(Secret);

            int minKeySizeBytes = 64;
            while (keyBytes.Length < minKeySizeBytes)
            {
                keyBytes = keyBytes.Concat(Encoding.UTF8.GetBytes(Secret)).ToArray();
            }

            var key = new SymmetricSecurityKey(keyBytes);

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(30),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public static string GetUsernameFromToken(string token)
        {
            var principal = GetPrincipal(token);
            if (principal == null)
                return null;

            var identity = principal.Identity as ClaimsIdentity;

            if (identity == null || !identity.IsAuthenticated)
                return null;

            var usernameClaim = identity.FindFirst(ClaimTypes.Name);
            var username = usernameClaim?.Value;

            return username;
        }

        private static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;
                byte[] keyBytes = Encoding.UTF8.GetBytes(Secret);

                int minKeySizeBytes = 64;
                while (keyBytes.Length < minKeySizeBytes)
                {
                    keyBytes = keyBytes.Concat(Encoding.UTF8.GetBytes(Secret)).ToArray();
                }

                var key = new SymmetricSecurityKey(keyBytes);

                var validationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = key
                };

                SecurityToken securityToken;
                var principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out securityToken);
                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}