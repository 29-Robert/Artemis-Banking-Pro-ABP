using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Features.LoanQuerys;
using ArtemisBankingPro.Application.Features.Loans.Commands;
using MediatR;
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
        private readonly IMediator _mediator;
        public LoanController(IMediator mediator)
        {
            _mediator = mediator;
        }
        // GET api/loan
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllLoansQuery());
            return Ok(result);
        }
        // GET api/loan/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetLoanByIdQuery { Id = id });
            if (result == null) return NotFound(new { Message = $"No se encontró un préstamo con Id {id}." });
            return Ok(result);
        }
        // POST api/loan
        [HttpPost]
        public async Task<IActionResult> AssignLoan([FromBody] CreateLoanRequestDto request)
        {
            var command = new AssignLoanCommand
            {
                ClientId = request.ClientId,
                CapitalAmount = request.CapitalAmount,
                TermInMonths = request.TermInMonths,
                AnnualInterestRate = request.AnnualInterestRate,
                ConfirmHighRisk = request.ConfirmHighRisk,
                AdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            };
            var result = await _mediator.Send(command);
            return Created($"api/loan/{result.Id}", result);
        }
        // PATCH api/loan/{id}/rate
        [HttpPatch("{id:int}/rate")]
        public async Task<IActionResult> UpdateRate(int id, [FromBody] UpdateLoanRateRequestDto request)
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