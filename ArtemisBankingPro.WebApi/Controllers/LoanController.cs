using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBankingPro.WebApi.Controllers
{
    [ApiController]
    [Route("api/loan")]
    [Authorize(Roles = "Administrador")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        // GET api/
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? cedula,
            [FromQuery] string? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _loanService.GetLoansAsync(cedula, status, pageNumber, pageSize);
            return Ok(result);
        }

        // GET api/loan
        [HttpGet("eligible-clients")]
        public async Task<IActionResult> GetEligibleClients(
            [FromQuery] string? cedula,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _loanService.GetEligibleClientsAsync(cedula, pageNumber, pageSize);
            return Ok(result);
        }

        // GET api/loan/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            
            var result = await _loanService.GetLoanByIdAsync(id);
            return Ok(result);
        }

        // POST api/loan
        [HttpPost]
        public async Task<IActionResult> AssignLoan([FromBody] CreateLoanRequestDto request)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _loanService.AssignLoanAsync(request, adminId);
            return Created($"api/loan/{result.Id}", result);
        }

        // PATCH api/loan/{id}/rate
        [HttpPatch("{id:int}/rate")]
        public async Task<IActionResult> UpdateRate(int id, [FromBody] UpdateLoanRateRequestDto request)
        {
            var result = await _loanService.UpdateInterestRateAsync(id, request.AnnualInterestRate);
            return Ok(result);
        }
    }
}