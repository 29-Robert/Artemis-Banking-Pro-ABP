using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.WebApp.Models;
using ArtemisBankingPro.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ArtemisBankingPro.Application.DTOs.Cashier; 

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

      
        public async Task<IActionResult> Index()
        {
            int cashierId = GetCashierId();
            var indicators = await _cashierService.GetHomeIndicatorsAsync(cashierId);
            return View(indicators);
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
               
                var dto = new DepositRequestDto
                {
                    DestinationAccountNumber = model.TargetAccountNumber,
                    Amount = model.Amount
                };

               
                var response = await _cashierService.ProcessDepositAsync(dto, GetCashierId());

                if (!response.Approved)
                {
                    ModelState.AddModelError(string.Empty, response.RejectionReason);
                    return View(model);
                }

                if (!string.IsNullOrEmpty(response.WarningMessage))
                    TempData["WarningMessage"] = response.WarningMessage;
                else
                    TempData["SuccessMessage"] = "Depósito realizado correctamente.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Withdrawal(WithdrawalViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var dto = new WithdrawRequestDto
                {
                    SourceAccountNumber = model.SourceAccountNumber,
                    Amount = model.Amount
                };

                var response = await _cashierService.ProcessWithdrawalAsync(dto, GetCashierId());

                if (!response.Approved)
                {
                    ModelState.AddModelError(string.Empty, response.RejectionReason);
                    return View(model);
                }

                if (!string.IsNullOrEmpty(response.WarningMessage))
                    TempData["WarningMessage"] = response.WarningMessage;
                else
                    TempData["SuccessMessage"] = "Retiro realizado correctamente.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreditCardPayment(CreditCardPaymentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var dto = new PayCreditCardRequestDto
                {
                    SourceAccountNumber = model.SourceAccountNumber,
                    CardNumber = model.CardNumber,
                    Amount = model.Amount
                };

                var response = await _cashierService.ProcessCreditCardPaymentAsync(dto, GetCashierId());

                if (!response.Approved)
                {
                    ModelState.AddModelError(string.Empty, response.RejectionReason);
                    return View(model);
                }

                if (!string.IsNullOrEmpty(response.WarningMessage))
                    TempData["WarningMessage"] = response.WarningMessage;
                else
                    TempData["SuccessMessage"] = "Pago a tarjeta realizado correctamente.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> LoanPayment(LoanPaymentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var dto = new PayLoanRequestDto
                {
                    SourceAccountNumber = model.SourceAccountNumber,
                    LoanNumber = model.LoanNumber,
                    Amount = model.Amount
                };

                var response = await _cashierService.ProcessLoanPaymentAsync(dto, GetCashierId());

                if (!response.Approved)
                {
                    ModelState.AddModelError(string.Empty, response.RejectionReason);
                    return View(model);
                }

                if (!string.IsNullOrEmpty(response.WarningMessage))
                    TempData["WarningMessage"] = response.WarningMessage;
                else
                    TempData["SuccessMessage"] = "Pago a préstamo realizado correctamente.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ThirdPartyTransfer(ThirdTransferViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var dto = new ThirdPartyTransferRequestDto
                {
                    SourceAccountNumber = model.SourceAccountNumber,
                    DestinationAccountNumber = model.TargetAccountNumber,
                    Amount = model.Amount
                };

                var response = await _cashierService.ProcessThirdPartyTransferAsync(dto, GetCashierId());

                if (!response.Approved)
                {
                    ModelState.AddModelError(string.Empty, response.RejectionReason);
                    return View(model);
                }

                if (!string.IsNullOrEmpty(response.WarningMessage))
                    TempData["WarningMessage"] = response.WarningMessage;
                else
                    TempData["SuccessMessage"] = "Transferencia realizada correctamente.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado: " + ex.Message);
                return View(model);
            }
        }

        private int GetCashierId()
        {
            var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claimValue, out int cashierId) ? cashierId : 0;
        }
    }
}