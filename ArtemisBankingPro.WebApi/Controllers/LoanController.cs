using Microsoft.AspNetCore.Mvc;
using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Features.Loans.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;




namespace ArtemisBankingPro.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")] // Regla de seguridad del módulo
    public class LoanController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoanController(IMediator mediator)
        {
            _mediator = mediator;
        }

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
                // Extraemos el ID del Admin logueado desde el JWT
                AdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            };

            var response = await _mediator.Send(command);

            // Retorna un 201 Created según el documento
            return Created("", response);
        }
    }
}