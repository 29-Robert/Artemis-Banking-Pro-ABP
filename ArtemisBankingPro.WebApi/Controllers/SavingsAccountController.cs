using ArtemisBankingPro.Application.Features.Accounts.Commands;
using ArtemisBankingPro.Application.Features.Accounts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApi.Controllers
{
    [ApiController]
    [Route("api/savings-account")]
    [Authorize(Roles = "Administrador")]
    public class SavingsAccountController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] ArtemisBankingPro.Domain.Enums.AccountStatus? status = null,
            [FromQuery] ArtemisBankingPro.Domain.Enums.AccountType? type = null,
            [FromQuery] string cedula = "")
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSecondary([FromBody] CreateSecondaryAccountCommand command)
        {
            try
            {
                var result = await mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{accountNumber}/transactions")]
        public async Task<IActionResult> GetTransactions(
            string accountNumber,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPatch("{accountNumber}/cancel")]
        public async Task<IActionResult> CancelSecondary(string accountNumber)
        {
            try
            {
                var command = new CancelSecondaryAccountCommand { AccountNumber = accountNumber };
                await mediator.Send(command);
                return Ok(new { Message = $"La cuenta secundaria {accountNumber} ha sido cancelada exitosamente y los fondos han sido transferidos al balance principal." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
