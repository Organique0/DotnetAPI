using DotnetAPI.Data;
using DotnetAPI.Models;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;


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
            userDB.Active = user.Active;
            userDB.FirstName = user.FirstName;
            userDB.LastName = user.LastName;
            userDB.Email = user.Email;
            userDB.Gender = user.Gender;

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

}


