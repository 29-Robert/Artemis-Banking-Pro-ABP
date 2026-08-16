using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Interfaces.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class CommerceController(ICommerceService commerceService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var result = await commerceService.GetPagedCommercesAsync(page, limit);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (!int.TryParse(id, out var intId) || intId <= 0)
            {
                return BadRequest(new { Message = "El formato del ID es inválido. Debe ser un entero positivo." });
            }

            var commerce = await commerceService.GetCommerceByIdAsync(intId);
            if(commerce == null)
            {
                return NotFound(new { Message = $"No se encontró el comercio con el ID {id}." });
            }
            return Ok(commerce);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommerceDto dto)
        {
            try
            {
                var result = await commerceService.CreateCommerceAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateCommerceDto dto)
        {
            if (!int.TryParse(id, out var intId) || intId <= 0)
            {
                return BadRequest(new { Message = "El formato del ID es inválido. Debe ser un entero positivo." });
            }

            try
            {
                await commerceService.UpdateCommerceAsync(intId, dto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Error = ex.Message });
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
                await commerceService.ChangeStatusAsync(intId, dto.IsActive);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
