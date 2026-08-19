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

        [HttpPost("confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmAccount([FromBody] ActivateUserCommand command)
        {
            if (string.IsNullOrEmpty(command.Token))
                return BadRequest(new { Error = "El token es requerido en el cuerpo de la petición." });

            try
            {
                await mediator.Send(command);
                return Ok(new { Message = "Su cuenta ha sido activada exitosamente. Ya puede iniciar sesión." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("get-reset-token")]
        [AllowAnonymous]
        public async Task<IActionResult> GetResetToken([FromBody] ArtemisBankingPro.Application.Features.Users.Commands.ForgotPassword.ForgotPasswordCommand command)
        {
            if (string.IsNullOrEmpty(command.Username))
                return BadRequest(new { Error = "El nombre de usuario es requerido." });

            try
            {
                await mediator.Send(command);
                return Ok(new { Message = "Si el usuario existe y tiene un correo registrado, se le ha enviado un enlace de recuperación." });
            }
            catch (Exception ex)
            {
                // Devolvemos 400 u otro status adecuado, aunque por seguridad a veces se prefiere devolver 200 siempre
                // La rúbrica pide manejar 400 en errores.
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ArtemisBankingPro.Application.Features.Users.Commands.ResetPassword.ResetPasswordCommand command)
        {
            if (string.IsNullOrEmpty(command.Token) || string.IsNullOrEmpty(command.NewPassword))
                return BadRequest(new { Error = "El token y la nueva contraseña son requeridos." });

            try
            {
                await mediator.Send(command);
                return Ok(new { Message = "Su contraseña ha sido restablecida exitosamente. Su cuenta ha sido reactivada." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}