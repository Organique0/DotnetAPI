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
    IUserRepository _userRepository;
    IMapper _mapper;

    public UserEFController(IConfiguration configuration, IUserRepository userRepository)
    {
        _entityFramework = new DataContextEF(configuration);
        _userRepository = userRepository;
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
        IEnumerable<User> users = _userRepository.GetUsers().ToList();
        return users;
    }

    [HttpGet("GetSigleUser/{userId}")]
    public User GetSigleUser(int userId)
    {
        /* User? user = _entityFramework.Users
        .Where(u => u.UserId == userId)
        .FirstOrDefault();
        if (user != null)
        {
            return user;
        }

        throw new Exception("Failed to get user"); */

        return _userRepository.GetSigleUser(userId);
    }

    [HttpPut("EditUser")]
    //IActionResult -> a response to tell you what happened (success, fail...)
    public IActionResult EditUser(User user)
    {
        User? userDB = _userRepository.GetSigleUser(user.UserId);

        if (userDB != null)
        {
            _mapper.Map(user, userDB);

            if (_userRepository.SaveChanges())
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


        _userRepository.AddEntity<User>(userDB);
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to update user");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userDB = _userRepository.GetSigleUser(userId);

        if (userDB != null)
        {
            _userRepository.RemoveEntity<User>(userDB);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
        }

        throw new Exception("Failed to delete user");
    }

    [HttpGet("GetUserSalary/{userId}")]
    public UserSalary GetUserSalary(int userId)
    {
        return _userRepository.GetSigleUserSalary(userId);
    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary(UserSalary userForInsert)
    {
        _userRepository.AddEntity<UserSalary>(userForInsert);

        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to add salary");
    }

    [HttpPut("UpdateUserSalary")]
    public IActionResult UpdateUserSalary(UserSalary userToUpdate)
    {
        UserSalary? userSalaryDB = _userRepository.GetSigleUserSalary(userToUpdate.UserId);

        if (userSalaryDB != null)
        {
            _mapper.Map(userToUpdate, userSalaryDB);

            if (_userRepository.SaveChanges())
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
        UserSalary? userSalaryDB = _userRepository.GetSigleUserSalary(userId);
        if (userSalaryDB != null)
        {
            _userRepository.RemoveEntity<UserSalary>(userSalaryDB);

            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to delete user salary");
        }

        throw new Exception("Could not find user salary to delete");
    }

    [HttpGet("GetUserJobInfo/{userId}")]
    public UserJobInfo GetUserJobInfo(int userId)
    {
        return _userRepository.GetUserJobInfo(userId);
    }

    [HttpPost("AddUserJobInfo")]
    public IActionResult AddUserJobInfo(UserJobInfo userForInsert)
    {
        _userRepository.AddEntity<UserJobInfo>(userForInsert);
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to add userInfo");
    }

    [HttpDelete("DeleteUserJobInfo")]
    public IActionResult DeleteUserInfo(int userId)
    {
        UserJobInfo? userJobInfoDB = _userRepository.GetUserJobInfo(userId);

        if (userJobInfoDB != null)
        {
            _userRepository.RemoveEntity<UserJobInfo>(userJobInfoDB);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
        }

        throw new Exception("Failed to delete user job info");
    }

    [HttpPut("UpdateUserJobInfo")]
    public IActionResult UpdateUserJobInfo(UserJobInfo userJobInfoToUpdate)
    {
        UserJobInfo? userJobInfoDB = _userRepository.GetUserJobInfo(userJobInfoToUpdate.UserId);

        if (userJobInfoDB != null)
        {
            _mapper.Map(userJobInfoToUpdate, userJobInfoDB);

            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to update user job info");
        }

        throw new Exception("Failed to find user job info to update");
    }
}


