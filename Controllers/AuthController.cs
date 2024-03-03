using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AutoMapper;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
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
        private readonly ReusableSql _reusableSql;
        private readonly IMapper _mapper;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _dapper = new DataContextDapper(configuration);
            _logger = logger;
            _authHelper = new AuthHelper(configuration);
            _reusableSql = new ReusableSql(configuration);
            _mapper = new Mapper(new MapperConfiguration(config =>
            {
                config.CreateMap<UserForRegistrationDTO, UserComplete>();
            }));
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
                    UserForLoginDTO userForSetPass = new()
                    {
                        Email = userForRegistration.Email,
                        Password = userForRegistration.Password
                    };
                    if (_authHelper.SetPassword(userForSetPass))
                    {
                        UserComplete userComplete = _mapper.Map<UserComplete>(userForRegistration);
                        userComplete.Active = true;

                        if (_reusableSql.UpsertUser(userComplete))
                        {
                            return Ok();
                        }
                    }

                    throw new Exception("Failed to register the user");
                }
                throw new Exception("User with this email already exist");
            }
            throw new Exception("Passwords do not match");

        }

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDTO userForSetPass)
        {
            if (_authHelper.SetPassword(userForSetPass))
            {
                return Ok();
            }

            throw new Exception("Failed to update password");
        }


        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDTO userForLogin)
        {
            try
            {
                string sqlForHashAndSalt = "EXEC TutorialAppSchema.sp_LoginConfirmation_Get @Email";

                //Use dynamic parameters instead. Just because.
                DynamicParameters sqlParameters = new();
                sqlParameters.Add("@Email", userForLogin.Email, DbType.String);

                UserForLoginConfirmationDto userAuthRow = _dapper
                .LoadDataSingleDynamic<UserForLoginConfirmationDto>(sqlForHashAndSalt, sqlParameters);

                byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userAuthRow.PasswordSalt);

                for (int index = 0; index < passwordHash.Length; index++)
                {
                    if (passwordHash[index] != userAuthRow.PasswordHash[index])
                    {
                        return StatusCode(401, "Password was not correct");
                    }
                }

                string userIdSql = "EXEC TutorialAppSchema.sp_UserId_Get @UserId, @Email";

                DynamicParameters UserIdSqlParameters = new();
                UserIdSqlParameters.Add("@Email", userForLogin.Email, DbType.String);
                UserIdSqlParameters.Add("@UserId", null, DbType.Int32);

                int userId = _dapper.LoadDataSingleDynamic<int>(userIdSql, UserIdSqlParameters);

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
            string userIdSql = "EXEC TutorialAppSchema.sp_UserId_Get @UserId, @Email=null";

            var userIdSqlParameters = new
            {
                UserId = Convert.ToInt32(User.FindFirst("userId")?.Value)
            };

            int userId = _dapper.LoadDataSingle<int>(userIdSql, userIdSqlParameters);
            //_logger.LogInformation(User.FindFirst("userId")?.Value);
            return _authHelper.CreateToken(userId);
        }


    }
}