using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.WebApp.Models;
using ArtemisBankingPro.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBankingPro.WebApp.Controllers
{
    [Authorize(Roles = "Cajero")]
    public class CashierController : Controller
    {
        private readonly ICashierService _cashierService;

        public CashierController(ICashierService cashierService)
        {
            _cashierService = cashierService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Deposit()
        {
            return View(new DepositViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(DepositViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _cashierService.ProcessDepositAsync(model.TargetAccountNumber, model.Amount, GetUserId());
                TempData["SuccessMessage"] = "Depósito realizado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Withdrawal()
        {
            return View(new WithdrawalViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Withdrawal(WithdrawalViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _cashierService.ProcessWithdrawalAsync(model.SourceAccountNumber, model.Amount, GetUserId());
                TempData["SuccessMessage"] = "Retiro realizado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult CreditCardPayment()
        {
            return View(new CreditCardPaymentViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreditCardPayment(CreditCardPaymentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _cashierService.ProcessCreditCardPaymentAsync(model.SourceAccountNumber, model.CardNumber, model.Amount, GetUserId());
                TempData["SuccessMessage"] = "Pago a tarjeta realizado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult LoanPayment()
        {
            return View(new LoanPaymentViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> LoanPayment(LoanPaymentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _cashierService.ProcessLoanPaymentAsync(model.SourceAccountNumber, model.LoanNumber, model.Amount, GetUserId());
                TempData["SuccessMessage"] = "Pago a préstamo realizado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ThirdPartyTransfer()
        {
            return View(new ThirdTransferViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ThirdPartyTransfer(ThirdTransferViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _cashierService.ProcessThirdPartyTransferAsync(model.SourceAccountNumber, model.TargetAccountNumber, model.Amount, GetUserId());
                TempData["SuccessMessage"] = "Transferencia realizada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }
    }
}