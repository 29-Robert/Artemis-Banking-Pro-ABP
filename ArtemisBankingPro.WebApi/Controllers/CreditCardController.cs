using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Features.CreditCard.Commands;
using ArtemisBankingPro.Application.Features.CreditCard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApi.Controllers
{
    /// <summary>
    /// Controlador para la gestión de tarjetas de crédito. Requiere rol de Administrador.
    /// </summary>
    [ApiController]
    [Route("api/credit-card")]
    [Authorize(Roles = "Administrador")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class CreditCardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CreditCardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtiene un listado paginado y filtrado de las tarjetas de crédito registradas.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<CreditCardResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Obtiene los detalles específicos de una tarjeta de crédito por su identificador único.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CreditCardResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetCreditCardByIdQuery { Id = id });
            if (result == null) return NotFound(new ProblemDetails { Detail = $"No se encontró tarjeta con Id {id}." });
            return Ok(result);
        }

        /// <summary>
        /// Asigna y crea una nueva tarjeta de crédito para un cliente específico.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CreditCardResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignCreditCard([FromBody] CreateCreditCardRequestDto request)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(adminIdClaim, out var adminId))
                return Unauthorized(new ProblemDetails { Detail = "No se pudo identificar al administrador desde el token." });

            var command = new AssignCreditCardCommand { ClientId = request.ClientId, CreditLimit = request.CreditLimit, AdminId = adminId };
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Actualiza el límite de crédito de una tarjeta de crédito existente.
        /// </summary>
        [HttpPatch("{id:int}/limit")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLimit(int id, [FromBody] UpdateCreditCardLimitRequestDto request)
        {
            var command = new UpdateCreditLimitCommand { CardId = id, NewCreditLimit = request.CreditLimit };
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Cancela una tarjeta de crédito activa.
        /// </summary>
        [HttpPatch("{id:int}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelCreditCard(int id)
        {
            await _mediator.Send(new CancelCreditCardCommand { CardId = id });
            return NoContent();
        }
    }
}