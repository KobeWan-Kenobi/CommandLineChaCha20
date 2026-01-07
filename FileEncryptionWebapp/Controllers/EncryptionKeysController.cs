//using FileEncryptionWebApp.DataAccess.EF.Context;
//using FileEncryptionWebApp.DataAccess.EF.Models;
//using FileEncryptionWebApp.ViewModels;
//using Microsoft.AspNetCore.Mvc;

//namespace FileEncryptionWebApp.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class UsersController : ControllerBase
//    {
//        private readonly FileEncryptionProjectDatabaseContext _context;

//        public UsersController(FileEncryptionProjectDatabaseContext context)
//        {
//            _context = context;
//        }

//        [HttpPost("register")]
//        public IActionResult Register([FromBody] UserRegistrationRequest request)
//        {
//            try
//            {
//                var viewModel = new UsersViewModel(_context);

//                // Check if username already exists
//                var existingUser = viewModel.GetAllUsers()
//                    .FirstOrDefault(u => u.Username == request.Username);

//                if (existingUser != null)
//                {
//                    return BadRequest(new { message = "Username already exists" });
//                }

//                var newUser = new User
//                {
//                    Username = request.Username,
//                    Password = request.Password // TODO: HASH THIS!
//                };

//                viewModel.SaveUser(newUser);

//                return Ok(new
//                {
//                    message = "User registered successfully",
//                    userId = newUser.UserId
//                });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "Registration failed", error = ex.Message });
//            }
//        }

//        [HttpPost("login")]
//        public IActionResult Login([FromBody] UserLoginRequest request)
//        {
//            try
//            {
//                var viewModel = new UsersViewModel(_context);
//                var user = viewModel.GetAllUsers()
//                    .FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);

//                if (user == null)
//                {
//                    return Unauthorized(new { message = "Invalid username or password" });
//                }

//                return Ok(new
//                {
//                    message = "Login successful",
//                    userId = user.UserId,
//                    username = user.Username
//                });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "Login failed", error = ex.Message });
//            }
//        }
//    }

//    public class UserRegistrationRequest
//    {
//        public string Username { get; set; }
//        public string Password { get; set; }
//    }

//    public class UserLoginRequest
//    {
//        public string Username { get; set; }
//        public string Password { get; set; }
//    }
//}