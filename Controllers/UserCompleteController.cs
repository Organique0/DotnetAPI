using DotnetAPI.Data;
using DotnetAPI.Models;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Dapper;
using System.Data;
using DotnetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;


namespace DotnetAPI.Controllers;
[Authorize]
[ApiController]
[Route("[controller]")]
//you can call a controller by the name before the word "Controller"
public class UserCompleteController : ControllerBase
{
    DataContextDapper _dapper;
    private readonly ReusableSql _reusableSql;

    public UserCompleteController(IConfiguration configuration)
    {
        _dapper = new DataContextDapper(configuration);
        _reusableSql = new ReusableSql(configuration);
    }

    [AllowAnonymous]
    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }
    [AllowAnonymous]
    [HttpGet("Test")]
    public string Test()
    {
        return "hello world";
    }

    //"value in {} will be displayed in API docs 
    [HttpGet("GetUsers/{userId}/{active}")]
    public IEnumerable<UserComplete> GetUsers(int userId, bool active)
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get";
        string parameters = "";

        DynamicParameters sqlParams = new();

        if (userId != 0)
        {
            parameters += ", @UserId = @UserIdParam";
            sqlParams.Add("@userIdParam", userId, DbType.Int32);
        }

        if (active)
        {
            parameters += ", @Active = @ActiveParameter";
            sqlParams.Add("@ActiveParameter", active, DbType.Boolean);
        }

        if (parameters.Length > 0)
        {
            sql += parameters[1..];
        }

        IEnumerable<UserComplete> users = _dapper.LoadDataDynamic<UserComplete>(sql, sqlParams);
        return users;
    }



    [HttpPut("UpsertUser")]
    //IActionResult -> a response to tell you what happened (success, fail...)
    public IActionResult UpsertUser(UserComplete user)
    {
        if (_reusableSql.UpsertUser(user))
        {
            return Ok();
        }

        throw new Exception("Failed to update user");
    }


    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = "EXEC TutorialAppSchema.spUser_Delete @UserId";
        DynamicParameters sqlParams = new();

        sqlParams.Add("@UserIdParameter", userId, DbType.Int32);

        if (_dapper.ExecuteSqlWithDynamicParameters(sql, sqlParams))
        {
            return Ok();
        }

        throw new Exception("Failed to delete user");
    }
}


