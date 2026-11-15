using Microsoft.AspNetCore.Mvc;
using TravelerNotes.Db;
using TravelerNotes.Models.User;

namespace TravelerNotes.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly TravelerNotesDb _db;
    public UsersController()
    {
        _db = new TravelerNotesDb();
    }

    [HttpGet("GetAllUsers")]
    public List<UserResponse> GetAllUsers()
    {
        return _db.Users.Select(i => new UserResponse
        {
            BirthDate = i.BirthDay,
            Email = i.Email,
            Name = i.Name,
            PhoneNumber = i.PhoneNumber,
            Surname = i.Surname,
        }).ToList();
    }

    [HttpGet("GetUserById")]
    public UserResponse GetUser(int id)
    {
        var user = _db.Users.FirstOrDefault(i => i.Id == id);
        if (user == null) throw new ArgumentException("no user found on this Id");
        return new UserResponse
        {
            BirthDate = user.BirthDay,
            Email = user.Email,
            Id = id,
            Name = user.Name,
            PhoneNumber = user.PhoneNumber,
            Surname = user.Surname,
        };
    }

    [HttpPost("AddNewUser")]
    public bool AddNewUser(UserRequest request)
    {
        if (request is null) throw new ArgumentException("request is null please check");
        if (request.Email?.ToLower()?.EndsWith("@gmail.com") == false) throw new ArgumentException("only gmails are allowed");

        _db.Users.Add(new Entitites.User
        {
            BirthDay = request.BirthDate,
            CodeWord = request.CodeWord,
            Name = request.Name,
            Email = request.Email,
            Password = request.Password,
            PhoneNumber = request.PhoneNumber,
            Surname = request.Surname,
        });
        _db.SaveChanges();

        return true;
    }

    [HttpDelete("DeleteUserById")]
    public bool DeleteuserById(int id)
    {
        var userExist = _db.Users.FirstOrDefault(u => u.Id == id);
        if (userExist == null) return false;
        _db.Users.Remove(userExist);
        _db.SaveChanges();
        return true;
    }

}
