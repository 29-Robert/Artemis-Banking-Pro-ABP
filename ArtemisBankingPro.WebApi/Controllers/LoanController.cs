using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Features.Loans.Commands;
using ArtemisBankingPro.Application.Features.Loans.Queries;
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

        [HttpGet]
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

        [HttpGet("eligible-clients")]
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetLoanByIdQuery
            {
                Id = id
            };

            var result = await _mediator.Send(query);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpGet("by-number/{loanNumber}")]
        public async Task<IActionResult> GetByNumber(string loanNumber)
        {
            var query = new GetLoanByNumberQuery
            {
                LoanNumber = loanNumber
            };

            var result = await _mediator.Send(query);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AssignLoan(
            [FromBody] CreateLoanRequestDto request)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized();
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

        [HttpPatch("{id:int}/rate")]
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