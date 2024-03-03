using DotnetAPI.Data;
using DotnetAPI.Models;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Dapper;
using System.Data;


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
        string sql = @"EXEC TutorialAppSchema.spUser_Upsert
            @FirstName = @FirstNameParameter,
            @LastName = @LastNameParameter,
            @Email = @EmailParameter,
            @Gender = @GenderParameter,
            @Active = @ActiveParameter,
            @JobTitle = @JobTitleParameter,
            @Department = @DepartmentParameter,
            @Salary = @SalaryParameter,
            @UserId = @UserIdParameter
            ";

        DynamicParameters sqlParams = new();

        sqlParams.Add("@FirstNameParameter", user.FirstName, DbType.String);
        sqlParams.Add("@LastNameParameter", user.LastName, DbType.String);
        sqlParams.Add("@EmailParameter", user.Email, DbType.String);
        sqlParams.Add("@GenderParameter", user.Gender, DbType.String);
        sqlParams.Add("@ActiveParameter", user.Active, DbType.String);
        sqlParams.Add("@JobTitleParameter", user.JobTitle, DbType.String);
        sqlParams.Add("@DepartmentParameter", user.Department, DbType.String);
        sqlParams.Add("@SalaryParameter", user.Salary, DbType.String);
        sqlParams.Add("@UserIdParameter", user.UserId, DbType.Int32);



        if (_dapper.ExecuteSqlWithDynamicParameters(sql, sqlParams))
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


