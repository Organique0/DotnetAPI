using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Controllers;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;
        private readonly DataContextDapper _dapper;

        public AuthHelper(IConfiguration config)
        {
            _config = config;
            _dapper = new DataContextDapper(config);
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

        public bool SetPassword(UserForLoginDTO userForSetPass)
        {
            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator r = RandomNumberGenerator.Create())
            {
                r.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = GetPasswordHash(userForSetPass.Password, passwordSalt);

            //inserting new value in auth table
            string sqlAddAuth = @"EXEC TutorialAppSchema.spRegistration_Upsert
                        @Email = @EmailParam,
                        @PasswordHash = @PasswordHashParam,
                        @PasswordSalt = @PasswordSaltParam
                    ";

            //a diffent approach of passing params is needed here
            //because of wierd types for pass hash and salt
            //also, you can use DynamicParameters here instead with some modifications
            List<SqlParameter> sqlParameters = [];

            SqlParameter passwordHashParam = new("@PasswordHashParam", SqlDbType.VarBinary)
            {
                Value = passwordHash
            };
            SqlParameter passwordSaltParam = new("@PasswordSaltParam", SqlDbType.VarBinary)
            {
                Value = passwordSalt
            };
            SqlParameter email = new("@EmailParam", SqlDbType.NVarChar)
            {
                Value = userForSetPass.Email
            };

            sqlParameters.Add(passwordSaltParam);
            sqlParameters.Add(passwordHashParam);
            sqlParameters.Add(email);

            return _dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters);
        }
    }
}