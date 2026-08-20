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
            var result = await mediator.Send(new ArtemisBankingPro.Application.Features.Users.Commands.Login.LoginCommand { Username = request.Username, Password = request.Password });
            return Ok(new { Jwt = result.Token });
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
            return NoContent();
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetResetToken([FromBody] ArtemisBankingPro.Application.Features.Users.Commands.ForgotPassword.ForgotPasswordCommand command)
        {
            if (string.IsNullOrEmpty(command.Username))
                throw new ArgumentException("El nombre de usuario es requerido.");

            await mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Restablece la contraseña del usuario con un token válido y reactiva su cuenta.
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ArtemisBankingPro.Application.Features.Users.Commands.ResetPassword.ResetPasswordCommand command)
        {
            if (string.IsNullOrEmpty(command.Token) || string.IsNullOrEmpty(command.NewPassword))
                throw new ArgumentException("El token y la nueva contraseña son requeridos.");

            await mediator.Send(command);
            return NoContent();
        }
    }
}
