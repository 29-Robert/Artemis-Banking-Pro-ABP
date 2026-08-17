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
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApi.Controllers
{
    [ApiController]
    [Route("pay")]
    [Authorize(Roles = "Administrador,Comercio")]
    public class PaymentController(
        IMediator mediator,
        ICurrentUserService currentUserService) : ControllerBase
    {
        [HttpPost("process-payment/{commerceId}")]
        public async Task<IActionResult> ProcessPayment(string commerceId, [FromBody] ProcessPaymentRequestDto request)
        {
            var userRole = currentUserService.Role;
            int resolvedCommerceId;

            if (userRole == "Comercio")
            {
                var jwtCommerceId = currentUserService.CommerceId;
                if (!jwtCommerceId.HasValue)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { Message = "El usuario no tiene un comercio asociado." });
                }
                resolvedCommerceId = jwtCommerceId.Value;
            }
            else // Administrador
            {
                if (!int.TryParse(commerceId, out var parsedCommerceId) || parsedCommerceId <= 0)
                {
                    return BadRequest(new { Message = "El formato del ID es inválido. Debe ser un entero positivo." });
                }

                var commerce = await mediator.Send(new GetCommerceByIdQuery { Id = parsedCommerceId });
                if (commerce == null)
                {
                    return NotFound(new { Message = $"El comercio con ID {commerceId} no existe." });
                }

                resolvedCommerceId = parsedCommerceId;
            }

            try
            {
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
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("get-transactions/{commerceId}")]
        public async Task<IActionResult> GetTransactions(string commerceId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var userRole = currentUserService.Role;
            int resolvedCommerceId;

            if (userRole == "Comercio")
            {
                var jwtCommerceId = currentUserService.CommerceId;
                if (!jwtCommerceId.HasValue)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { Message = "El usuario no tiene un comercio asociado." });
                }
                resolvedCommerceId = jwtCommerceId.Value;
            }
            else // Administrador
            {
                if (!int.TryParse(commerceId, out var parsedCommerceId) || parsedCommerceId <= 0)
                {
                    return BadRequest(new { Message = "El formato del ID es inválido. Debe ser un entero positivo." });
                }

                var commerce = await mediator.Send(new GetCommerceByIdQuery { Id = parsedCommerceId });
                if (commerce == null)
                {
                    return NotFound(new { Message = $"El comercio con ID {commerceId} no existe." });
                }

                resolvedCommerceId = parsedCommerceId;
            }

            try
            {
                var query = new GetTransactionsQuery
                {
                    CommerceId = resolvedCommerceId,
                    Page = page,
                    Limit = limit
                };
                var result = await mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
