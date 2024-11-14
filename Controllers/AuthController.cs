using System.Data;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using nett.Data;
using nett.Dtos;
using nett.Helpers;

namespace nett.Controllers;

[Authorize]
public class AuthController(IConfiguration config) : ControllerBase
{
    private readonly DataContext _dapper = new(config);
    private readonly AuthHelper _authHelper = new(config);

    [AllowAnonymous]
    [HttpPost("Register")]
    public IActionResult Register(UserForRegistrationDto userForRegistration)
    {
        if (userForRegistration.Password != userForRegistration.PasswordConfirm)
        {
            throw new Exception("Passwords do not match");
        }

        var sqlCheckUserExists = "SELECT * FROM TutorialAppSchema.Auth WHERE Email = '" + userForRegistration.Email + "'";
        IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
        if (existingUsers.Count() > 0)
        {
            throw new Exception("User with this Email Allready Exists");
        }

        byte[] passwordSalt = new byte[128 / 8];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetNonZeroBytes(passwordSalt);
        }

        byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

        string sqlAddAuth = @"INSERT INTO TutorialAppSchema.Auth ([Email], [PasswordHash], [PasswordSalt]) VALUES ('" + userForRegistration.Email + "', @PasswordHash, @PasswordSalt)";

        List<SqlParameter> sqlParameters = [];
        SqlParameter passwordSaltParameter = new("@PasswordSalt", SqlDbType.VarBinary)
        {
            Value = passwordSalt
        };

        SqlParameter passwordHashParameter = new("@PasswordHash", SqlDbType.VarBinary)
        {
            Value = passwordHash
        };

        sqlParameters.Add(passwordSaltParameter);
        sqlParameters.Add(passwordHashParameter);

        if (!_dapper.ExecuteSqlWithParamenter(sqlAddAuth, sqlParameters))
        {
            throw new Exception("Failed to register user");
        }
        string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users 
        ([FirstName], [LastName], [Email], [Gender], [Active])
        VALUES 
        ('" + userForRegistration.FirstName + @"', '" + userForRegistration.LastName + @"', '" + userForRegistration.Email + @"', '" + userForRegistration.Gender + @"', 1)";

        if (!_dapper.ExecuteSql(sqlAddUser))
        {
            throw new Exception("Failed to add user");
        }

        return Ok("Suggessfully registered!");
    }


    [AllowAnonymous]
    [HttpPost("Login")]
    public IActionResult Login(UserForLoginDto userForLogin)
    {
        try
        {
            var sqlForHashAndSalt = @"SELECT [Email], [PasswordHash], [PasswordSalt] FROM TutorialAppSchema.Auth WHERE [Email] = '" + userForLogin.Email + "'";
            UserForLoginConfirmationDto userConfirmation = _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);

            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userConfirmation.PasswordSalt);

            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userConfirmation.PasswordHash[index])
                {
                    return StatusCode(401, "Incorrect password");
                }
            }

            string userIdSql = "SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '" + userForLogin.Email + "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string> {
               {"token", _authHelper.CreateToken(userId)}
            });
        }
        catch (System.Exception err)
        {
            Console.WriteLine(err);
            return NotFound("Server Error 500");
        }
    }


    [HttpGet("RefreshToken")]
    public string RefreshToken()
    {
        string userIdSql = @"SELECT UserId FROM TutorialAppSchema.Users WHERE UserId = '" + User.FindFirst("userId")?.Value + "'";
        int userId = _dapper.LoadDataSingle<int>(userIdSql);

        return _authHelper.CreateToken(userId);
    }
}