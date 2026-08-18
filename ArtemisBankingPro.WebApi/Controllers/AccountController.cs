using ArtemisBankingPro.Application.DTOs;
using ArtemisBankingPro.Application.Features.Users.Commands.ActivateUser;
using ArtemisBankingPro.Application.Features.Users.Commands.CreateUser;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApi.Controllers
{
    /// <summary>
    /// Controlador para la autenticación, registro y activación de cuentas de usuario.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public class AccountController(
        IGenericRepository<User> userRepository,
        IJwtService jwtService,
        IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Inicia sesión de usuario y retorna un token JWT válido.
        /// </summary>
        /// <param name="request">Credenciales del usuario.</param>
        /// <returns>Token JWT de acceso.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request) 
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                throw new ArgumentException("Faltan parámetros requeridos.");

            var allUsers = await userRepository.GetAllAsync();
            var user = allUsers.FirstOrDefault(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Su cuenta se encuentra inactiva. Debe activar su cuenta antes de iniciar sesión.");

            if (user.RoleId != 1 && user.RoleId != 4)
                throw new UnauthorizedAccessException("Acceso denegado. No tiene permisos para utilizar este recurso.");

            var token = jwtService.GenerateToken(user);
            return Ok(new { Jwt = token });
        }

        /// <summary>
        /// Registra un nuevo usuario en la plataforma. Envía un correo con un token de activación.
        /// </summary>
        /// <param name="command">Datos del usuario a registrar.</param>
        /// <returns>Mensaje de éxito y ID del usuario creado.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] CreateUserCommand command)
        {
            var userId = await mediator.Send(command);
            return Ok(new { Message = "Usuario registrado exitosamente. Revise su correo para la activación.", UserId = userId });
        }

        /// <summary>
        /// Activa la cuenta de un usuario recién registrado utilizando el token de activación recibido por correo.
        /// </summary>
        /// <param name="token">Token de activación recibido por correo.</param>
        /// <returns>Mensaje de confirmación de activación.</returns>
        [HttpGet("activate")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ActivateAccount([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("El token es requerido.");

            var command = new ActivateUserCommand { Token = token };
            await mediator.Send(command);

            return Ok(new { Message = "Su cuenta ha sido activada exitosamente. Ya puede iniciar sesión." });
        }
    }
}