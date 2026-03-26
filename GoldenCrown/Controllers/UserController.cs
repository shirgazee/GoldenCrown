using FluentValidation;
using GoldenCrown.API.Dtos;
using GoldenCrown.Application.Dtos.User;
using GoldenCrown.Application.Features.User.UserLogin;
using GoldenCrown.Application.Features.User.UserRegister;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GoldenCrown.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, [FromServices] IValidator<RegisterRequest> validator)
        {
            var validationResult = validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var command = new UserRegisterCommand(request.Login, request.Name, request.Password);
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                return Ok();
            }
            return BadRequest(new { Message = result.ErrorMessage });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromServices] IValidator<LoginRequest> validator)
        {
            var validationResult = validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var command = new UserLoginCommand(request.Login, request.Password);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(new { Token = result.Value });
            }
            return NotFound();
        }
    }
}