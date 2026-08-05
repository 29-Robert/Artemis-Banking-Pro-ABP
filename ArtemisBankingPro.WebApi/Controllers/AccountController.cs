using ArtemisBankingPro.Application.DTOs;
using ArtemisBankingPro.Application.Features.Users.Commands.ActivateUser;
using ArtemisBankingPro.Application.Features.Users.Commands.CreateUser;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(
        IGenericRepository<User> userRepository,
        IJwtService jwtService,
        IMediator mediator) : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request) 
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Faltan parámetros requeridos.");

            var allUsers = await userRepository.GetAllAsync();
            var user = allUsers.FirstOrDefault(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Credenciales inválidas.");

            if (!user.IsActive)
                return Unauthorized("Su cuenta se encuentra inactiva. Debe activar su cuenta antes de iniciar sesión.");

            if (user.RoleId != 1 && user.RoleId != 4)
                return StatusCode(403, "Acceso denegado. No tiene permisos para utilizar este recurso.");

            var token = jwtService.GenerateToken(user);
            return Ok(new { Jwt = token });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateUserCommand command)
        {
            try
            {
                var userId = await mediator.Send(command);
                return Ok(new { Message = "Usuario registrado exitosamente. Revise su correo para la activación.", UserId = userId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("activate")]
        [AllowAnonymous]
        public async Task<IActionResult> ActivateAccount([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return BadRequest(new { Error = "El token es requerido." });

                var command = new ActivateUserCommand { Token = token };
                await mediator.Send(command);

                return Ok(new { Message = "Su cuenta ha sido activada exitosamente. Ya puede iniciar sesión." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}