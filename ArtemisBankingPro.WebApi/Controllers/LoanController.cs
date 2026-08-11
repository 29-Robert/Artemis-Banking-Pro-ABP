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
        // GET api/loan
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _loanService.GetAllLoansAsync();
            return Ok(result);
        }
        // GET api/loan/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _loanService.GetLoanByIdAsync(id);
            if (result == null) return NotFound(new { Message = $"No se encontró un préstamo con Id {id}." });
            return Ok(result);
        }
        // POST api/loan
        [HttpPost]
        public async Task<IActionResult> AssignLoan([FromBody] CreateLoanRequestDto request)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _loanService.AssignLoanAsync(request, adminId);
            return Created($"api/loan/{result.Id}", result);
        }
        // PATCH api/loan/{id}/rate
        [HttpPatch("{id:int}/rate")]
        public async Task<IActionResult> UpdateRate(int id, [FromBody] UpdateLoanRateRequestDto request)
        {
            await _loanService.UpdateLoanRateAsync(id, request.AnnualInterestRate);
            return NoContent();
        }
    }
}