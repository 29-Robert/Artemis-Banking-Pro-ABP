using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Features.CreditCard.Commands;
using ArtemisBankingPro.Application.Features.CreditCard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBankingPro.WebApi.Controllers
{
    [ApiController]
    [Route("api/credit-card")]
    [Authorize(Roles = "Administrador")]
    public class CreditCardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CreditCardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? cedula,
            [FromQuery] string? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetAllCreditCardsQuery { Cedula = cedula, Status = status, PageNumber = pageNumber, PageSize = pageSize };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetCreditCardByIdQuery { Id = id });
            if (result == null) return NotFound(new { Message = $"No se encontró tarjeta con Id {id}." });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AssignCreditCard([FromBody] CreateCreditCardRequestDto request)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(adminIdClaim, out var adminId))
                return Unauthorized(new { Message = "No se pudo identificar al administrador desde el token." });

            var command = new AssignCreditCardCommand { ClientId = request.ClientId, CreditLimit = request.CreditLimit, AdminId = adminId };
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPatch("{id:int}/limit")]
        public async Task<IActionResult> UpdateLimit(int id, [FromBody] UpdateCreditCardLimitRequestDto request)
        {
            var command = new UpdateCreditLimitCommand { CardId = id, NewCreditLimit = request.CreditLimit };
            await _mediator.Send(command);
            return NoContent();
        }

       
        [HttpPatch("{id:int}/cancel")]
        public async Task<IActionResult> CancelCreditCard(int id)
        {
            await _mediator.Send(new CancelCreditCardCommand { CardId = id });
            return NoContent();
        }
    }
}