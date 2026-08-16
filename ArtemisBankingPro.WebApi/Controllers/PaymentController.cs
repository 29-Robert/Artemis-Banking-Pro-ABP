using ArtemisBankingPro.Application.DTOs.HermesPay;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApi.Controllers
{
    [ApiController]
    [Route("pay")]
    [Authorize(Roles = "Administrador,Comercio")]
    public class PaymentController(
        IPaymentService paymentService,
        ICurrentUserService currentUserService,
        ICommerceService commerceService) : ControllerBase
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

                var commerce = await commerceService.GetCommerceByIdAsync(parsedCommerceId);
                if (commerce == null)
                {
                    return NotFound(new { Message = $"El comercio con ID {commerceId} no existe." });
                }

                resolvedCommerceId = parsedCommerceId;
            }

            try
            {
                var result = await paymentService.ProcessPaymentAsync(resolvedCommerceId, request, currentUserService.UserId);

                if (result.Status == TransactionStatus.Rechazada)
                {
                    return UnprocessableEntity(result);
                }

                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new { Error = ex.Message });
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

                var commerce = await commerceService.GetCommerceByIdAsync(parsedCommerceId);
                if (commerce == null)
                {
                    return NotFound(new { Message = $"El comercio con ID {commerceId} no existe." });
                }

                resolvedCommerceId = parsedCommerceId;
            }

            try
            {
                var result = await paymentService.GetTransactionsAsync(resolvedCommerceId, page, limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
