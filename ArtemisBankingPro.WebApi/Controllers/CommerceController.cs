using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Features.Commerces.Commands;
using ArtemisBankingPro.Application.Features.Commerces.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApi.Controllers
{
    /// <summary>
    /// Controlador para la gestión de comercios afiliados al sistema.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public class CommerceController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Obtiene un listado paginado de los comercios afiliados.
        /// </summary>
        /// <param name="page">Número de la página actual.</param>
        /// <param name="limit">Cantidad máxima de registros por página.</param>
        /// <returns>Listado paginado de comercios.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var query = new GetPagedCommercesQuery { Page = page, Limit = limit };
            var result = await mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene los detalles de un comercio por su identificador único.
        /// </summary>
        /// <param name="id">ID del comercio.</param>
        /// <returns>Los detalles del comercio consultado.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            if (!int.TryParse(id, out var intId) || intId <= 0)
            {
                throw new ArgumentException("El formato del ID es inválido. Debe ser un entero positivo.");
            }

            var commerce = await mediator.Send(new GetCommerceByIdQuery { Id = intId });
            if (commerce == null)
            {
                throw new KeyNotFoundException($"No se encontró el comercio con el ID {id}.");
            }
            return Ok(commerce);
        }

        /// <summary>
        /// Crea e inscribe un nuevo comercio en el sistema, generándole una cuenta de ahorros principal.
        /// </summary>
        /// <param name="command">Datos necesarios para crear el comercio.</param>
        /// <returns>El comercio creado con su ID y número de cuenta principal asignado.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCommerceCommand command)
        {
            var result = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Actualiza los datos generales de un comercio existente. Los cambios de estado no se permiten por este método.
        /// </summary>
        /// <param name="id">ID del comercio a actualizar.</param>
        /// <param name="command">Nuevos datos del comercio.</param>
        /// <returns>Sin contenido (NoContent) en caso de éxito.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateCommerceCommand command)
        {
            if (!int.TryParse(id, out var intId) || intId <= 0)
            {
                throw new ArgumentException("El formato del ID es inválido. Debe ser un entero positivo.");
            }

            command.Id = intId;
            await mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Activa o desactiva el estado de un comercio en el sistema.
        /// </summary>
        /// <param name="id">ID del comercio a modificar.</param>
        /// <param name="dto">Nuevo estado activo/inactivo.</param>
        /// <returns>Sin contenido (NoContent) en caso de éxito.</returns>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] UpdateCommerceStatusDto dto)
        {
            if (!int.TryParse(id, out var intId) || intId <= 0)
            {
                throw new ArgumentException("El formato del ID es inválido. Debe ser un entero positivo.");
            }

            var command = new ChangeCommerceStatusCommand { Id = intId, IsActive = dto.IsActive };
            await mediator.Send(command);
            return NoContent();
        }
    }
}
