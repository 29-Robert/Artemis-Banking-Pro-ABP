using ArtemisBankingPro.Application.Features.Accounts.Commands;
using ArtemisBankingPro.Application.Features.Accounts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApi.Controllers
{
    /// <summary>
    /// Controlador para la gestión de cuentas de ahorro.
    /// </summary>
    [ApiController]
    [Route("api/savings-account")]
    [Authorize(Roles = "Administrador")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public class SavingsAccountController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Obtiene un listado paginado de cuentas de ahorro filtrado por estado, tipo o cédula del cliente.
        /// </summary>
        /// <param name="page">Número de página.</param>
        /// <param name="pageSize">Tamaño de la página.</param>
        /// <param name="status">Filtrar por estado de la cuenta.</param>
        /// <param name="type">Filtrar por tipo de cuenta.</param>
        /// <param name="cedula">Filtrar por cédula del titular.</param>
        /// <returns>Listado paginado de cuentas de ahorro.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] ArtemisBankingPro.Domain.Enums.AccountStatus? status = null,
            [FromQuery] ArtemisBankingPro.Domain.Enums.AccountType? type = null,
            [FromQuery] string cedula = "")
        {
            var query = new GetAccountsQuery
            {
                Page = page,
                PageSize = pageSize,
                Status = status,
                Type = type,
                Cedula = cedula
            };
            var result = await mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Crea una nueva cuenta de ahorro secundaria para un cliente existente.
        /// </summary>
        /// <param name="command">Datos para la creación de la cuenta secundaria.</param>
        /// <returns>Detalles de la cuenta de ahorro creada.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSecondary([FromBody] CreateSecondaryAccountCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene el historial de transacciones de una cuenta de ahorros específica de forma paginada.
        /// </summary>
        /// <param name="accountNumber">Número de cuenta de ahorros.</param>
        /// <param name="page">Número de página.</param>
        /// <param name="pageSize">Tamaño de la página.</param>
        /// <returns>Historial paginado de transacciones.</returns>
        [HttpGet("{accountNumber}/transactions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransactions(
            string accountNumber,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetStatementQuery
            {
                AccountNumber = accountNumber,
                Page = page,
                PageSize = pageSize
            };
            var history = await mediator.Send(query);
            return Ok(history);
        }

        /// <summary>
        /// Cancela una cuenta de ahorro secundaria y transfiere sus fondos restantes a la cuenta de ahorro principal.
        /// </summary>
        /// <param name="accountNumber">Número de cuenta secundaria a cancelar.</param>
        /// <returns>Mensaje confirmando la cancelación y transferencia.</returns>
        [HttpPatch("{accountNumber}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelSecondary(string accountNumber)
        {
            var command = new CancelSecondaryAccountCommand { AccountNumber = accountNumber };
            await mediator.Send(command);
            return Ok(new { Message = $"La cuenta secundaria {accountNumber} ha sido cancelada exitosamente y los fondos han sido transferidos al balance principal." });
        }
    }
}
