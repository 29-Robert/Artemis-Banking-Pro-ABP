using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApi.Controllers
{
    [ApiController]
    [Route("api/savings-account")]
    [Authorize(Roles = "Administrador")]
    public class SavingsAccountController(
        ISavingsAccountService accountService,
        ISavingsAccountRepository accountRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] AccountStatus? status = null,
            [FromQuery] AccountType? type = null,
            [FromQuery] string cedula = "")
        {
            try
            {
                var result = await accountRepository.GetPagedAsync(page, pageSize, status, type, cedula);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSecondary([FromBody] CreateSecondaryAccountDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await accountService.CreateSecondaryAccountAsync(dto);
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
                var history = await accountService.GetTransactionHistoryAsync(accountNumber, page, pageSize);
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
                await accountService.CancelSecondaryAccountAsync(accountNumber);
                return Ok(new { Message = $"La cuenta secundaria {accountNumber} ha sido cancelada exitosamente y los fondos han sido transferidos al balance principal." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
