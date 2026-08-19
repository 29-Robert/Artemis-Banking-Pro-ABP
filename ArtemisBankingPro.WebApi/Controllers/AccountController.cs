using ArtemisBankingPro.Application.DTOs;
using ArtemisBankingPro.Application.Features.Users.Commands.ActivateUser;
using ArtemisBankingPro.Application.Features.Users.Commands.CreateUser;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        IUserRepository userRepository,
        IJwtService jwtService,
        IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Autentica un usuario (Administrador o Comercio) y genera un token JWT.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request) 
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                throw new ArgumentException("Faltan parámetros requeridos.");

            var user = await userRepository.GetByUsernameAsync(request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Su cuenta se encuentra inactiva. Debe activar su cuenta antes de iniciar sesión.");

            if (user.RoleId != (int)ArtemisBankingPro.Domain.Enums.Roles.Administrador && user.RoleId != (int)ArtemisBankingPro.Domain.Enums.Roles.Comercio)
                throw new UnauthorizedAccessException("Acceso denegado. No tiene permisos para utilizar este recurso.");

            var token = jwtService.GenerateToken(user);
            return Ok(new { Jwt = token });
        }

        /// <summary>
        /// Registra un nuevo usuario y envía correo de activación.
        /// </summary>
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
        /// Activa una cuenta de usuario mediante un token enviado en el cuerpo de la petición.
        /// </summary>
        [HttpPost("confirm")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmAccount([FromBody] ActivateUserCommand command)
        {
            if (string.IsNullOrEmpty(command.Token))
                throw new ArgumentException("El token es requerido en el cuerpo de la petición.");

            await mediator.Send(command);
            return Ok(new { Message = "Su cuenta ha sido activada exitosamente. Ya puede iniciar sesión." });
        }

        /// <summary>
        /// Activa una cuenta de usuario mediante un token en el query string (para enlaces de correo).
        /// </summary>
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

        /// <summary>
        /// Solicita un token de restablecimiento de contraseña. Inactiva temporalmente al usuario.
        /// </summary>
        [HttpPost("get-reset-token")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetResetToken([FromBody] ArtemisBankingPro.Application.Features.Users.Commands.ForgotPassword.ForgotPasswordCommand command)
        {
            if (string.IsNullOrEmpty(command.Username))
                throw new ArgumentException("El nombre de usuario es requerido.");

            await mediator.Send(command);
            return Ok(new { Message = "Si el usuario existe y tiene un correo registrado, se le ha enviado un enlace de recuperación." });
        }

        /// <summary>
        /// Restablece la contraseña del usuario con un token válido y reactiva su cuenta.
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ArtemisBankingPro.Application.Features.Users.Commands.ResetPassword.ResetPasswordCommand command)
        {
            if (string.IsNullOrEmpty(command.Token) || string.IsNullOrEmpty(command.NewPassword))
                throw new ArgumentException("El token y la nueva contraseña son requeridos.");

            await mediator.Send(command);
            return Ok(new { Message = "Su contraseña ha sido restablecida exitosamente. Su cuenta ha sido reactivada." });
        }
    }
}
