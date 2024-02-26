using DotnetAPI.Data;
using DotnetAPI.Models;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;


namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
//you can call a controller by the name before the word "Controller"
public class UserCompleteController : ControllerBase
{
    DataContextDapper _dapper;
    public UserCompleteController(IConfiguration configuration)
    {
        _dapper = new DataContextDapper(configuration);
    }

    //"value in {} will be displayed in API docs 
    [HttpGet("GetUsers/{userId}/{active}")]
    public IEnumerable<UserComplete> GetUsers(int userId, bool active)
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get";
        string parameters = "";

        parameters += userId != 0 ? ", @UserId=" + userId.ToString() : "";
        parameters += active ? ", @Active=" + active.ToString() : "";

        if (parameters.Length > 0)
        {
            sql += parameters[1..];
        }

        IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql);
        return users;
    }



    [HttpPut("UpsertUser")]
    //IActionResult -> a response to tell you what happened (success, fail...)
    public IActionResult UpsertUser(UserComplete user)
    {
        string sql = @"EXEC TutorialAppSchema.spUser_Upsert
            @FirstName,
            @LastName,
            @Email,
            @Gender,
            @Active,
            @JobTitle,
            @Department,
            @Salry,
            @UserId
            ";

        var parameters = new
        {
            user.FirstName,
            user.LastName,
            user.Email,
            user.Gender,
            user.Active,
            user.UserId
        };

        if (_dapper.ExecuteSql(sql, parameters))
        {
            return Ok();
        }

        throw new Exception("Failed to update user");
    }


    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = "EXEC TutorialAppSchema.spUser_Delete @UserId";

        var parameters = new
        {
            UserId = userId
        };

        if (_dapper.ExecuteSql(sql, parameters))
        {
            return Ok();
        }

        throw new Exception("Failed to delete user");
    }
}


