using DotnetAPI.Data;
using DotnetAPI.Models;
using DotnetAPI.Dtos;
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
    public IActionResult AddUser(UserToAddDTO user)
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

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"
            DELETE FROM TutorialAppSchema.Users WHERE UserId = @UserId
        ";

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

    [HttpGet("GetUserSalary/{userId}")]
    public UserSalary GetUserSalary(int userId)
    {
        string sql = @"
            SELECT UserId, Salary FROM TutorialAppSchema.UserSalary 
            WHERE UserId = @UserId 
        ";

        var parameters = new
        {
            UserId = userId
        };

        return _dapper.LoadDataSingle<UserSalary>(sql, parameters);
    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary(UserSalary userForInsert)
    {
        string sql = @"
            INSERT INTO TutorialAppSchema.UserSalary(
                [UserId],
                [Salary]
            ) VALUES (
                @UserId,
                @Salary
            )
       ";

        var parameters = new
        {
            userForInsert.UserId,
            userForInsert.Salary,
        };

        if (_dapper.ExecuteSql(sql, parameters))
        {
            return Ok();
        }

        throw new Exception("Failed to add user salary");
    }

    [HttpPut("UpdateUserSalary")]
    public IActionResult UpdateUserSalary(UserSalary userToUpdate)
    {
        string sql = @"
            UPDATE TutorialAppSchema.UserSalary
            SET Salary = @Salary
            WHERE UserId = @UserId
        ";

        var parameters = new
        {
            userToUpdate.UserId,
            userToUpdate.Salary,
        };

        if (_dapper.ExecuteSql(sql, parameters))
        {
            return Ok();
        }

        throw new Exception("Failed to update users salary");

    }

    [HttpDelete("DeleteUserSalary/{userId}")]
    public IActionResult DeleteUserSalary(int userId)
    {
        string sql = @"
            DELETE FROM TutorialAppSchema.UserSalary
            WHERE UserId = @userId
        ";

        var parameters = new
        {
            userId
        };

        if (_dapper.ExecuteSql(sql, parameters))
        {
            return Ok();
        }

        throw new Exception("Failed to delete user salary");
    }

    [HttpGet("GetUserJobInfo/{userId}")]
    public UserJobInfo GetUserJobInfo(int userId)
    {
        string sql = @"
            SELECT [UserId],[JobTitle], [Department]
            FROM TutorialAppSchema.UserJobInfo
            WHERE UserId = @userId
        ";

        var parameters = new
        {
            userId
        };

        return _dapper.LoadDataSingle<UserJobInfo>(sql, parameters);
    }

    [HttpPost("AddUserJobInfo")]
    public IActionResult AddUserJobInfo(UserJobInfo userForInsert)
    {
        string sql = @"
            INSERT INTO TutorialAppSchema.UserJobInfo (
                UserId,
                Department,
                JobTitle
            ) VALUES (
                @userId,
                @department,
                @jobTitle
            ) 
        ";

        var parameters = new
        {
            userForInsert.UserId,
            userForInsert.Department,
            userForInsert.JobTitle
        };

        if (_dapper.ExecuteSql(sql, parameters))
        {
            return Ok();
        }

        throw new Exception("Failed to add userJobInfo");
    }

    [HttpDelete("DeleteUserJobInfo")]
    public IActionResult DeleteUserInfo(int userId)
    {
        string sql = @"
            DELETE FROM TutorialAppSchema.UserJobInfo
            WHERE UserId = @userId 
        ";

        var parameters = new
        {
            userId
        };

        if (_dapper.ExecuteSql(sql, parameters))
        {
            return Ok();
        }

        throw new Exception("Failed to delete user jobInfo");
    }

    [HttpPut("UpdateUserJobInfo")]
    public IActionResult UpdateUserJobInfo(UserJobInfo userJobInfoToUpdate)
    {
        string sql = @"
            UPDATE TutorialAppSchema.UserJobInfo 
            SET JobTitle = @JobTitle, Department = @Department
            WHERE UserId = @UserId;
        ";

        var parameters = new
        {
            userJobInfoToUpdate.JobTitle,
            userJobInfoToUpdate.Department,
            userJobInfoToUpdate.UserId,
        };

        if (_dapper.ExecuteSql(sql, parameters))
        {
            return Ok();
        }

        throw new Exception("Failed to update user job info");
    }
}


