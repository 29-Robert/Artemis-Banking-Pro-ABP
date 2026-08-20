using ArtemisBankingPro.Application.Exceptions;
using ArtemisBankingPro.Application.Features.Loans.Commands;
using ArtemisBankingPro.Application.Features.Loans.Queries;
using ArtemisBankingPro.WebApp.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBankingPro.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class LoanController(IMediator mediator) : Controller
    {
        // GET: /Loan/Index?cedula=...&status=...&page=1
        [HttpGet]
        public async Task<IActionResult> Index(string? cedula, string? status, int page = 1)
        {
            var query = new GetLoansQuery
            {
                Cedula = cedula,
                Status = status,
                PageNumber = page,
                PageSize = 20
            };

            var result = await mediator.Send(query);

            ViewBag.Cedula = cedula;
            ViewBag.Status = status;
            ViewBag.CurrentPage = result.PageNumber;
            ViewBag.TotalPages = result.TotalPages;

            return View(result.Items);
        }

        // GET: /Loan/Assign
        [HttpGet]
        public IActionResult Assign()
        {
            return View(new AssignLoanViewModel());
        }

        // POST: /Loan/Assign
        [HttpPost]
        public async Task<IActionResult> Assign(AssignLoanViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

                var command = new AssignLoanCommand
                {
                    ClientId = vm.ClientId,
                    CapitalAmount = vm.CapitalAmount,
                    TermInMonths = vm.TermInMonths,
                    AnnualInterestRate = vm.AnnualInterestRate,
                    ConfirmHighRisk = vm.ConfirmHighRisk,
                    AdminId = adminId
                };

                await mediator.Send(command);
                TempData["SuccessMessage"] = "Préstamo asignado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (HighRiskClientException ex)
            {
                
                TempData["HighRiskMessage"] = ex.Message;
                TempData["HighRiskType"] = ex.RiskType;
                TempData["HighRiskCurrentDebt"] = ex.CurrentDebt.ToString("N2");
                TempData["HighRiskProjectedDebt"] = ex.ProjectedDebt.ToString("N2");
                TempData["HighRiskAverageDebt"] = ex.AverageDebt.ToString("N2");

                
                vm.ConfirmHighRisk = false;
                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        // GET: /Loan/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var result = await mediator.Send(new GetLoanByIdQuery { Id = id });
            if (result == null) return NotFound();

            return View(result);
        }

        // GET: /Loan/EditRate/5
        [HttpGet]
        public async Task<IActionResult> EditRate(int id)
        {
            var loan = await mediator.Send(new GetLoanByIdQuery { Id = id });
            if (loan == null) return NotFound();

            var vm = new EditLoanRateViewModel
            {
                LoanId = loan.Id,
                LoanNumber = loan.LoanNumber,
                CurrentRate = loan.AnnualInterestRate
            };

            return View(vm);
        }

        // POST: /Loan/EditRate
        [HttpPost]
        public async Task<IActionResult> EditRate(EditLoanRateViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                await mediator.Send(new UpdateLoanRateCommand
                {
                    LoanId = vm.LoanId,
                    NewAnnualInterestRate = vm.NewRate
                });

                TempData["SuccessMessage"] = "Tasa de interés actualizada correctamente.";
                return RedirectToAction(nameof(Details), new { id = vm.LoanId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }
    }
}
