using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;
        public AuthHelper(IConfiguration config)
        {
            _config = config;
        }

        public byte[] GetPasswordHash(string pass, byte[] passSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value
            + Convert.ToBase64String(passSalt);

            return KeyDerivation.Pbkdf2(
                password: pass,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            );
        }

        public string CreateToken(int userId)
        {
            Claim[] claims = [
                new Claim("userId", userId.ToString())
            ];

            //pull data from appsettings
            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

            SymmetricSecurityKey tokenKey = new(
                    Encoding.UTF8.GetBytes(
                        //tokenKeyString != null ? tokenKeyString : ""
                        tokenKeyString ?? ""
                    )
            );

            SigningCredentials credentials = new(
                tokenKey,
                SecurityAlgorithms.HmacSha512Signature
            );

            //new SecurityTokenDescripto(){}
            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler tokenHandler = new();

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);

        }
    }
}