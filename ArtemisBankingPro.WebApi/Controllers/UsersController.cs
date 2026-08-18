using ArtemisBankingPro.Application.Features.Users.Commands.CreateUser;
using ArtemisBankingPro.Application.Features.Users.Commands.ToggleUserStatus;
using ArtemisBankingPro.Application.Features.Users.Commands.UpdateUser;
using ArtemisBankingPro.Application.Features.Users.Queries;
using ArtemisBankingPro.Application.Features.Users.Queries.GetAllUsers;
using ArtemisBankingPro.Application.Features.Users.Queries.GetCommerceUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBankingPro.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class UsersController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
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

        [HttpGet("commerce")]
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

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            if (command.RoleId == 4)
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

        [HttpPost("commerce/{commerceId}")]
        public async Task<IActionResult> CreateCommerceUser(int commerceId, [FromBody] CreateUserCommand command)
        {
            try
            {
                command.RoleId = 4; // Asegurarse de que sea rol Comercio
                command.CommerceId = commerceId;

                var userId = await mediator.Send(command);
                return CreatedAtAction(nameof(GetUserById), new { id = userId }, new { Message = "Usuario de comercio creado exitosamente.", UserId = userId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
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

        [HttpPatch("{id}/status")]
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

        [HttpGet("{id}")]
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
