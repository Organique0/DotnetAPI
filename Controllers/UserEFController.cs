using DotnetAPI.Data;
using DotnetAPI.Models;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.Xml;


namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
//you can call a controller by the name before the word "Controller"
public class UserEFController : ControllerBase
{
    DataContextEF _entityFramework;
    IMapper _mapper;

    public UserEFController(IConfiguration configuration)
    {
        _entityFramework = new DataContextEF(configuration);
        _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            //type to map from. type to map to.
            cfg.CreateMap<UserToAddDTO, User>();
            cfg.CreateMap<UserSalary, UserSalary>();
            cfg.CreateMap<UserJobInfo, UserJobInfo>();
        }));
    }

    //"value in {} will be displayed in API docs 
    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _entityFramework.Users.ToList();
        return users;
    }

    [HttpGet("GetSigleUser/{userId}")]
    public User GetSigleUser(int userId)
    {
        User? user = _entityFramework.Users
        .Where(u => u.UserId == userId)
        .FirstOrDefault();
        if (user != null)
        {
            return user;
        }

        throw new Exception("Failed to get user");
    }

    [HttpPut("EditUser")]
    //IActionResult -> a response to tell you what happened (success, fail...)
    public IActionResult EditUser(User user)
    {
        User? userDB = _entityFramework.Users
       .Where(u => u.UserId == user.UserId)
       .FirstOrDefault();

        if (userDB != null)
        {
            _mapper.Map(user, userDB);

            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }

            throw new Exception("Failed to update user");
        }

        throw new Exception("Failed to get user");

    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDTO user)
    {
        User userDB = _mapper.Map<User>(user);

        //without automapper
        /*      userDB.Active = user.Active;
                userDB.FirstName = user.FirstName;
                userDB.LastName = user.LastName;
                userDB.Email = user.Email;
                userDB.Gender = user.Gender; */


        _entityFramework.Add(userDB);
        if (_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }

        throw new Exception("Failed to update user");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userDB = _entityFramework.Users
        .Where(d => d.UserId == userId)
        .FirstOrDefault();

        if (userDB != null)
        {
            _entityFramework.Users.Remove(userDB);
            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }
        }

        throw new Exception("Failed to delete user");
    }

    [HttpGet("GetUserSalary/{userId}")]
    public IEnumerable<UserSalary> GetUserSalary(int userId)
    {
        return _entityFramework.UserSalary
        .Where(u => u.UserId == userId)
        .ToList();
    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary(UserSalary userForInsert)
    {
        _entityFramework.UserSalary.Add(userForInsert);
        if (_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }

        throw new Exception("Failed to add salary");
    }

    [HttpPut("UpdateUserSalary")]
    public IActionResult UpdateUserSalary(UserSalary userToUpdate)
    {
        UserSalary? userSalaryDB = _entityFramework.UserSalary
        .FirstOrDefault(u => u.UserId == userToUpdate.UserId);

        if (userSalaryDB != null)
        {
            _mapper.Map(userToUpdate, userSalaryDB);

            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }

            throw new Exception("Failed to update user salary");
        }

        throw new Exception("Failed to find user salary to update");
    }

    [HttpDelete("DeleteUserSalary/{userId}")]
    public IActionResult DeleteUserSalary(int userId)
    {
        UserSalary? userSalaryDB = _entityFramework.UserSalary
        .FirstOrDefault(u => u.UserId == userId);

        if (userSalaryDB != null)
        {
            _entityFramework.UserSalary.Remove(userSalaryDB);

            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }

            throw new Exception("Failed to delete user salary");
        }

        throw new Exception("Could not find user salary to delete");
    }

    [HttpGet("GetUserJobInfo/{userId}")]
    public IEnumerable<UserJobInfo> GetUserJobInfo(int userId)
    {
        return _entityFramework.UserJobInfo
        .Where(u => u.UserId == userId)
        .ToList();
    }

    [HttpPost("AddUserJobInfo")]
    public IActionResult AddUserJobInfo(UserJobInfo userForInsert)
    {
        _entityFramework.UserJobInfo.Add(userForInsert);
        if (_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }

        throw new Exception("Failed to add userInfo");
    }

    [HttpDelete("DeleteUserJobInfo")]
    public IActionResult DeleteUserInfo(int userId)
    {
        UserJobInfo? userJobInfoDB = _entityFramework.UserJobInfo
        .FirstOrDefault(u => u.UserId == userId);

        if (userJobInfoDB != null)
        {
            _entityFramework.UserJobInfo.Remove(userJobInfoDB);
            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }
        }

        throw new Exception("Failed to delete user job info");
    }

    [HttpPut("UpdateUserJobInfo")]
    public IActionResult UpdateUserJobInfo(UserJobInfo userJobInfoToUpdate)
    {
        UserJobInfo? userJobInfoDB = _entityFramework.UserJobInfo
        .FirstOrDefault(u => u.UserId == userJobInfoToUpdate.UserId);

        if (userJobInfoDB != null)
        {
            _mapper.Map(userJobInfoToUpdate, userJobInfoDB);

            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }

            throw new Exception("Failed to update user job info");
        }

        throw new Exception("Failed to find user job info to update");
    }
}


