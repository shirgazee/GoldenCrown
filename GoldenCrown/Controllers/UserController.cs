using GoldenCrown.Dtos.User;
using GoldenCrown.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoldenCrown.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if(ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.RegisterAsync(request.Login, request.Name, request.Password);
            if (result)
            {
                return Ok();
            }
            return BadRequest(new { Message = "User registration failed" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if(ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }
            var result = await _userService.LoginAsync(request.Login, request.Password);
            if (result)
            {
                return Ok(new { Token = result.Value });
            }
            return NotFound();
        }
    }
}