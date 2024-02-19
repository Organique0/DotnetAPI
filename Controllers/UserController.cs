using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
//you can call a controller by the name before the word "Controller"
public class UserController : ControllerBase
{
    DataContextDapper _dapper;
    public UserController(IConfiguration configuration)
    {
        _dapper = new DataContextDapper(configuration);
    }

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    //"value in {} will be displayed in API docs 
    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        string sql = @"
        SELECT [UserId],
            [FirstName],
            [LastName],
            [Email],
            [Gender],
            [Active]
        FROM TutorialAppSchema.Users;
        ";

        IEnumerable<User> users = _dapper.LoadData<User>(sql);
        return users;
    }

    [HttpGet("GetSigleUser/{userId}")]
    public User GetSigleUser(int userId)
    {
        string sql = @"
        SELECT [UserId],
            [FirstName],
            [LastName],
            [Email],
            [Gender],
            [Active]
        FROM TutorialAppSchema.Users
        WHERE UserId = @UserId";

        var parameters = new
        {
            UserId = userId
        };

        User user = _dapper.LoadDataSingle<User>(sql, parameters);
        return user;
    }

    [HttpPut("EditUser")]
    //IActionResult -> a response to tell you what happened (success, fail...)
    public IActionResult EditUser(User user)
    {
        string sql = @"
        UPDATE TutorialAppSchema.Users 
        SET [FirstName] = @FirstName,
            [LastName] = @LastName,
            [Email] = @Email,
            [Gender] = @Gender,
            [Active] = @Active
        WHERE UserId = @UserId";

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

    [HttpPost("AddUser")]
    public IActionResult AddUser(User user)
    {
        string sql = @"
             INSERT INTO TutorialAppSchema.Users
                (
                [FirstName],
                [LastName],
                [Email],
                [Gender],
                [Active]
                )
            VALUES
            (
               @FirstName,
               @LastName,
               @Email,
               @Gender,
               @Active 
            );
        ";

        var parameters = new
        {
            user.FirstName,
            user.LastName,
            user.Email,
            user.Gender,
            user.Active
        };

        if (_dapper.ExecuteSql(sql, parameters))
        {
            return Ok();
        }

        throw new Exception("Failed to add user");
    }

}


