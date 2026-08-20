using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs;
using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Features.Loans.Commands;
using ArtemisBankingPro.Application.Features.Loans.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApi.Controllers
{
    /// <summary>
    /// Controlador para la administración de préstamos. Requiere rol de Administrador.
    /// </summary>
    [ApiController]
    [Route("api/loan")]
    [Authorize(Roles = "Administrador")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class LoanController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoanController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtiene un listado paginado y filtrado de los préstamos registrados.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<LoanResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get(
            [FromQuery] string? cedula,
            [FromQuery] string? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetLoansQuery
            {
                Cedula = cedula,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene un listado paginado de clientes elegibles para solicitar préstamos.
        /// </summary>
        [HttpGet("eligible-clients")]
        [ProducesResponseType(typeof(PagedUserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetEligibleClients(
            [FromQuery] string? cedula,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetEligibleClientsQuery
            {
                Cedula = cedula,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene la información detallada de un préstamo por su identificador único.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(LoanResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetLoanByIdQuery
            {
                Id = id
            };

            var result = await _mediator.Send(query);
            if (result == null) return NotFound(new ProblemDetails { Detail = $"No se encontró el préstamo con ID {id}." });

            return Ok(result);
        }

        /// <summary>
        /// Obtiene la información de un préstamo por su número identificador único de préstamo.
        /// </summary>
        [HttpGet("by-number/{loanNumber}")]
        [ProducesResponseType(typeof(LoanResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByNumber(string loanNumber)
        {
            var query = new GetLoanByNumberQuery
            {
                LoanNumber = loanNumber
            };

            var result = await _mediator.Send(query);
            if (result == null) return NotFound(new ProblemDetails { Detail = $"No se encontró el préstamo con número {loanNumber}." });

            return Ok(result);
        }

        /// <summary>
        /// Asigna y crea un nuevo préstamo para un cliente, generando su tabla de amortización correspondiente.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(LoanResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AssignLoan(
            [FromBody] CreateLoanRequestDto request)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized(new ProblemDetails { Detail = "No se pudo identificar al administrador desde el token." });
            }

            var command = new AssignLoanCommand
            {
                ClientId = request.ClientId,
                CapitalAmount = request.CapitalAmount,
                TermInMonths = request.TermInMonths,
                AnnualInterestRate = request.AnnualInterestRate,
                ConfirmHighRisk = request.ConfirmHighRisk,
                AdminId = adminId
            };

            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Actualiza la tasa de interés anual de un préstamo activo, recalculando las cuotas futuras pendientes.
        /// </summary>
        [HttpPatch("{id:int}/rate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRate(
            int id,
            [FromBody] UpdateLoanRateRequestDto request)
        {
            var command = new UpdateLoanRateCommand
            {
                LoanId = id,
                NewAnnualInterestRate = request.AnnualInterestRate
            };

            await _mediator.Send(command);

            return NoContent();
        }
    }
}