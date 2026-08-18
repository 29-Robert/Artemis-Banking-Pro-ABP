using ArtemisBankingPro.Application.DTOs.HermesPay;
using ArtemisBankingPro.Application.Features.Commerces.Queries;
using ArtemisBankingPro.Application.Features.HermesPay.Commands;
using ArtemisBankingPro.Application.Features.HermesPay.Queries;
using ArtemisBankingPro.Application.Interfaces.Services;
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
    /// Controlador para procesar cobros y consultar transacciones a través de la pasarela Hermes Pay.
    /// </summary>
    [ApiController]
    [Route("pay")]
    [Authorize(Roles = "Administrador,Comercio")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public class PaymentController(
        IMediator mediator,
        ICurrentUserService currentUserService) : ControllerBase
    {
        /// <summary>
        /// Procesa un pago/consumo a través de una tarjeta de crédito en Hermes Pay.
        /// </summary>
        /// <param name="commerceId">ID del comercio destino (ignorado para usuarios con rol Comercio).</param>
        /// <param name="request">Datos de la tarjeta de crédito y detalles del pago.</param>
        /// <returns>La información del consumo procesado, incluyendo código de autorización si aplica.</returns>
        [HttpPost("process-payment/{commerceId}")]
        [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ProcessPayment(string commerceId, [FromBody] ProcessPaymentRequestDto request)
        {
            var userRole = currentUserService.Role;
            int resolvedCommerceId;

            if (userRole == "Comercio")
            {
                var jwtCommerceId = currentUserService.CommerceId;
                if (!jwtCommerceId.HasValue)
                {
                    throw new UnauthorizedAccessException("El usuario no tiene un comercio asociado.");
                }
                resolvedCommerceId = jwtCommerceId.Value;
            }
            else // Administrador
            {
                if (!int.TryParse(commerceId, out var parsedCommerceId) || parsedCommerceId <= 0)
                {
                    throw new ArgumentException("El formato del ID es inválido. Debe ser un entero positivo.");
                }

                var commerce = await mediator.Send(new GetCommerceByIdQuery { Id = parsedCommerceId });
                if (commerce == null)
                {
                    throw new KeyNotFoundException($"El comercio con ID {commerceId} no existe.");
                }

                resolvedCommerceId = parsedCommerceId;
            }

            var command = new ProcessPaymentCommand
            {
                CommerceId = resolvedCommerceId,
                CardNumber = request.CardNumber,
                ExpirationMonth = request.ExpirationMonth,
                ExpirationYear = request.ExpirationYear,
                Cvc = request.Cvc,
                Amount = request.Amount,
                Description = request.Description,
                UserId = currentUserService.UserId
            };

            var result = await mediator.Send(command);

            if (result.Status == ArtemisBankingPro.Domain.Enums.TransactionStatus.Rechazada)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtiene un historial paginado de transacciones y consumos de tarjetas de crédito procesadas por un comercio.
        /// </summary>
        /// <param name="commerceId">ID del comercio (ignorado para usuarios con rol Comercio).</param>
        /// <param name="page">Número de página actual.</param>
        /// <param name="limit">Cantidad máxima de registros por página.</param>
        /// <returns>Listado paginado de transacciones del comercio.</returns>
        [HttpGet("get-transactions/{commerceId}")]
        [ProducesResponseType(typeof(PagedTransactionsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTransactions(string commerceId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var userRole = currentUserService.Role;
            int resolvedCommerceId;

            if (userRole == "Comercio")
            {
                var jwtCommerceId = currentUserService.CommerceId;
                if (!jwtCommerceId.HasValue)
                {
                    throw new UnauthorizedAccessException("El usuario no tiene un comercio asociado.");
                }
                resolvedCommerceId = jwtCommerceId.Value;
            }
            else // Administrador
            {
                if (!int.TryParse(commerceId, out var parsedCommerceId) || parsedCommerceId <= 0)
                {
                    throw new ArgumentException("El formato del ID es inválido. Debe ser un entero positivo.");
                }

                var commerce = await mediator.Send(new GetCommerceByIdQuery { Id = parsedCommerceId });
                if (commerce == null)
                {
                    throw new KeyNotFoundException($"El comercio con ID {commerceId} no existe.");
                }

                resolvedCommerceId = parsedCommerceId;
            }

            var query = new GetTransactionsQuery
            {
                CommerceId = resolvedCommerceId,
                Page = page,
                Limit = limit
            };
            var result = await mediator.Send(query);
            return Ok(result);
        }
    }
}
