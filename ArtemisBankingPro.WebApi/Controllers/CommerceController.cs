using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Features.Commerces.Commands;
using ArtemisBankingPro.Application.Features.Commerces.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class CommerceController(IMediator mediator) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var query = new GetPagedCommercesQuery { Page = page, Limit = limit };
            var result = await mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (!int.TryParse(id, out var intId) || intId <= 0)
            {
                return BadRequest(new { Message = "El formato del ID es inválido. Debe ser un entero positivo." });
            }

            var commerce = await mediator.Send(new GetCommerceByIdQuery { Id = intId });
            if (commerce == null)
            {
                return NotFound(new { Message = $"No se encontró el comercio con el ID {id}." });
            }
            return Ok(commerce);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommerceCommand command)
        {
            try
            {
                var result = await mediator.Send(command);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateCommerceCommand command)
        {
            if (!int.TryParse(id, out var intId) || intId <= 0)
            {
                return BadRequest(new { Message = "El formato del ID es inválido. Debe ser un entero positivo." });
            }

            try
            {
                command.Id = intId;
                await mediator.Send(command);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] UpdateCommerceStatusDto dto)
        {
            if (!int.TryParse(id, out var intId) || intId <= 0)
            {
                return BadRequest(new { Message = "El formato del ID es inválido. Debe ser un entero positivo." });
            }

            try
            {
                var command = new ChangeCommerceStatusCommand { Id = intId, IsActive = dto.IsActive };
                await mediator.Send(command);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
