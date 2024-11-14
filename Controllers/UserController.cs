using Microsoft.AspNetCore.Mvc;
using nett.Data;
using nett.Models;

namespace nett.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController(IConfiguration config) : ControllerBase
{
    readonly DataContext _dapper = new(config);

    [HttpGet]
    public IEnumerable<User> GetUsers()
    {
        string sql = "SELECT * FROM TutorialAppSchema.Users";
        return _dapper.LoadData<User>(sql);
    }

    [HttpPost]
    public IActionResult PostData(User newUser)
    {
        string sql = @"SET IDENTITY_INSERT TutorialAppSchema.Users ON;
        INSERT INTO TutorialAppSchema.Users 
        ([UserId], [FirstName], [LastName], [Email], [Gender], [Active])
        VALUES 
        (" + newUser.UserId + @", '" + newUser.FirstName + @"', '" + newUser.LastName + @"', '" + newUser.Email + @"', '" + newUser.Gender + @"', '" + newUser.Active + @"');
        SET IDENTITY_INSERT TutorialAppSchema.Users OFF;";
        _dapper.ExecuteSql(sql);
        return Ok("Added Successfully");
    }
}
