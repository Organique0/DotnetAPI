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
        WHERE UserId = " + userId.ToString() + ";";

        User user = _dapper.LoadDataSingle<User>(sql);
        return user;
    }



}


