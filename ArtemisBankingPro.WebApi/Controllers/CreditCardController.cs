using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Features.CreditCard;
using ArtemisBankingPro.Application.Features.CreditCardQuerys;
using ArtemisBankingPro.Domain.Entities;
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
        // GET api/credit-card
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllCreditCardsQuery());
            return Ok(result);
        }
        // GET api/credit-card/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetCreditCardByIdQuery { Id = id });
            if (result == null) return NotFound(new { Message = $"No se encontró tarjeta con Id {id}." });
            return Ok(result);
        }
        // POST api/credit-card
        [HttpPost]
        public async Task<IActionResult> AssignCreditCard([FromBody] CreateCreditCardRequestDto request)
        {
            var command = new AssignCreditCardCommand
            {
                ClientId = request.ClientId,
                CreditLimit = request.CreditLimit,
                AdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            };
            var result = await _mediator.Send(command);
            return Created($"api/credit-card/{result.Id}", result);
        }
        // PATCH api/credit-card/{id}/limit
        [HttpPatch("{id:int}/limit")]
        public async Task<IActionResult> UpdateLimit(int id, [FromBody] UpdateCreditCardLimitRequestDto request)
        {
            var command = new UpdateCreditLimitCommand
            {
                CardId = id,
                NewCreditLimit = request.CreditLimit
            };
            await _mediator.Send(command);
            return NoContent();
        }
        // DELETE api/credit-card/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelCreditCard(int id)
        {
            await _mediator.Send(new CancelCreditCardCommand { CardId = id });
            return NoContent();
        }
    }
}
