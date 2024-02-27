using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Controllers
{
    //require authentication
    [Authorize]
    [ApiController]
    [Route("[Controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly ILogger _logger;
        private readonly AuthHelper _authHelper;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _dapper = new DataContextDapper(configuration);
            _logger = logger;
            _authHelper = new AuthHelper(configuration);
        }

        //allow non-authorized access to this endpoint
        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDTO userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = @"
                    SELECT Email from TutorialAppSchema.Auth where Email = @Email; 
                ";

                var parameters = new
                {
                    userForRegistration.Email
                };

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists, parameters);

                if (!existingUsers.Any())
                {
                    //some complex password stuff
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator r = RandomNumberGenerator.Create())
                    {
                        r.GetNonZeroBytes(passwordSalt);
                    }

                    byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

                    //inserting new value in auth table
                    string sqlAddAuth = @"EXEC TutorialAppSchema.spRegistration_Upsert
                        @Email = @EmailParam,
                        @PasswordHash = @PasswordHashParam,
                        @PasswordSalt = @PasswordSaltParam
                    ";

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
                        Value = userForRegistration.Email
                    };

                    sqlParameters.Add(passwordSaltParam);
                    sqlParameters.Add(passwordHashParam);
                    sqlParameters.Add(email);

                    if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
                    {
                        string UserToAddSql = @"
                            EXEC TutorialAppSchema.spUser_Upsert
                                @FirstName,
                                @LastName,
                                @Email,
                                @Gender,
                                @JobTitle,
                                @Department,
                                @Salary,
                                @Active
                        ";

                        var UserToAddParameters = new
                        {
                            userForRegistration.FirstName,
                            userForRegistration.LastName,
                            userForRegistration.Email,
                            userForRegistration.Gender,
                            userForRegistration.JobTitle,
                            userForRegistration.Department,
                            userForRegistration.Salary,
                            Active = 1
                        };


                        try
                        {
                            _dapper.ExecuteSql(UserToAddSql, UserToAddParameters);
                            return Ok();
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.Message);
                        }

                        throw new Exception("Failed to add user");
                    }

                    throw new Exception("Failed to register the user");
                }
                throw new Exception("User with this email already exist");
            }
            throw new Exception("Passwords do not match");

        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDTO userForLogin)
        {
            try
            {
                string sqlForHashAndSalt = @"
                SELECT [PasswordHash], [PasswordSalt] from TutorialAppSchema.Auth 
                WHERE Email = @Email;  
                ";

                var parameters = new
                {
                    userForLogin.Email
                };
                UserForLoginConfirmationDto userAuthRow = _dapper
                .LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt, parameters);

                byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userAuthRow.PasswordSalt);

                for (int index = 0; index < passwordHash.Length; index++)
                {
                    if (passwordHash[index] != userAuthRow.PasswordHash[index])
                    {
                        return StatusCode(401, "Password was not correct");
                    }
                }

                string userIdSql = @"
                    SELECT UserId FROM TutorialAppSchema.Users
                    WHERE Email = @Email
                ";

                var userIdSqlParameters = new
                {
                    userForLogin.Email
                };

                int userId = _dapper.LoadDataSingle<int>(userIdSql, userIdSqlParameters);

                return Ok(new Dictionary<string, string>
                {
                    {"token",_authHelper.CreateToken(userId)}
                });
            }
            catch (Exception e)
            {
                //return StatusCode(500, "No user with this email found");
                throw new Exception(e.Message);
            }
        }

        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            string userIdSql = @"
                    SELECT UserId FROM TutorialAppSchema.Users
                    WHERE UserId = @UserId
                ";

            var userIdSqlParameters = new
            {
                UserId = User.FindFirst("userId")?.Value
            };

            int userId = _dapper.LoadDataSingle<int>(userIdSql, userIdSqlParameters);
            //_logger.LogInformation(User.FindFirst("userId")?.Value);
            return _authHelper.CreateToken(userId);
        }


    }
}