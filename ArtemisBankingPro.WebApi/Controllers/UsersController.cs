using ArtemisBankingPro.Application.Features.Users.Commands.CreateUser;
using ArtemisBankingPro.Application.Features.Users.Commands.ToggleUserStatus;
using ArtemisBankingPro.Application.Features.Users.Commands.UpdateUser;
using ArtemisBankingPro.Application.Features.Users.Queries;
using ArtemisBankingPro.Application.Features.Users.Queries.GetAllUsers;
using ArtemisBankingPro.Application.Features.Users.Queries.GetCommerceUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApi.Controllers
{
    /// <summary>
    /// Controlador para la gestión administrativa de usuarios en el sistema.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public class UsersController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Obtiene un listado de todos los usuarios registrados, filtrado opcionalmente por rol.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] string? roleFilter, [FromQuery] int page = 1)
        {
            var query = new GetAllUsersQuery
            {
                RoleFilter = roleFilter,
                Page = page,
                PageSize = 20
            };

            var result = await mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un listado paginado de los usuarios vinculados a comercios.
        /// </summary>
        [HttpGet("commerce")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCommerceUsers([FromQuery] int page = 1)
        {
            var query = new GetCommerceUsersQuery
            {
                Page = page,
                PageSize = 20
            };

            var result = await mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema. No se permite crear rol Comercio por este endpoint.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            if (command.RoleId == (int)ArtemisBankingPro.Domain.Enums.Roles.Comercio)
                return BadRequest(new { Error = "Para crear un usuario comercio debe usar el endpoint /api/users/commerce/{commerceId}" });

            try
            {
                var userId = await mediator.Send(command);
                return CreatedAtAction(nameof(GetUserById), new { id = userId }, new { Message = "Usuario creado exitosamente.", UserId = userId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo usuario asociado a un comercio específico.
        /// </summary>
        [HttpPost("commerce/{commerceId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCommerceUser(int commerceId, [FromBody] CreateUserCommand command)
        {
            try
            {
                command.RoleId = 4;
                command.CommerceId = commerceId;

                var userId = await mediator.Send(command);
                return CreatedAtAction(nameof(GetUserById), new { id = userId }, new { Message = "Usuario de comercio creado exitosamente.", UserId = userId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza la información general de un usuario existente.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserCommand command)
        {
            if (id != command.Id)
                return BadRequest(new { Error = "El ID en la ruta no coincide con el ID del cuerpo." });

            try
            {
                await mediator.Send(command);
                return Ok(new { Message = "Usuario actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Activa o desactiva el estado de acceso de un usuario.
        /// </summary>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var currentUserIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(currentUserIdClaim, out int currentUserId) && currentUserId == id)
            {
                return BadRequest(new { Error = "No puede cambiar su propio estado." });
            }

            try
            {
                var command = new ToggleUserStatusCommand { UserId = id };
                var newState = await mediator.Send(command);
                var statusText = newState ? "activado" : "inactivado";
                return Ok(new { Message = $"Usuario {statusText} exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene la información de un usuario por su identificador único.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(int id)
        {
            var query = new GetUserByIdQuery { Id = id };
            var user = await mediator.Send(query);

            if (user == null)
                return NotFound(new { Error = "Usuario no encontrado." });

            return Ok(user);
        }
    }
}
