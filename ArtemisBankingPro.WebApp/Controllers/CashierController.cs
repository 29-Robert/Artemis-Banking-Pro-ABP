using ArtemisBankingPro.Application.Features.Accounts.Commands;
using ArtemisBankingPro.Application.Features.Accounts.Queries;
using ArtemisBankingPro.WebApp.ViewModels;
using ArtemisBankingPro.WebApp.Models;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ArtemisBankingPro.Application.Interfaces.Services;

namespace ArtemisBankingPro.WebApp.Controllers
{
    [Authorize(Roles = "Cajero")]
    public class CashierController(IMediator mediator, IMapper mapper, ICashierService cashierService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            int cashierId = GetCashierId();
            var indicators = await mediator.Send(new GetHomeIndicatorsQuery { CashierId = cashierId });
            return View(indicators);
        }

        // --- DEPÓSITO ---
        [HttpGet]
        public IActionResult Deposit() => View(new DepositViewModel());

        [HttpPost]
        public async Task<IActionResult> Deposit(DepositViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var command = mapper.Map<DepositCommand>(model);
                command.CashierId = GetCashierId();
                var response = await mediator.Send(command);

                if (!response.Approved)
                {
                    ModelState.AddModelError(string.Empty, response.RejectionReason);
                    return View(model);
                }

                TempData["SuccessMessage"] = string.IsNullOrEmpty(response.WarningMessage) ? "Depósito realizado correctamente." : response.WarningMessage;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado: " + ex.Message);
                return View(model);
            }
        }

        // --- RETIRO ---
        [HttpGet]
        public IActionResult Withdrawal() => View(new WithdrawalViewModel());

        [HttpPost]
        public async Task<IActionResult> Withdrawal(WithdrawalViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var command = mapper.Map<WithdrawCommand>(model);
                command.CashierId = GetCashierId();
                var response = await mediator.Send(command);

                if (!response.Approved)
                {
                    ModelState.AddModelError(string.Empty, response.RejectionReason);
                    return View(model);
                }

                TempData["SuccessMessage"] = string.IsNullOrEmpty(response.WarningMessage) ? "Retiro realizado correctamente." : response.WarningMessage;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado: " + ex.Message);
                return View(model);
            }
        }

        // --- PAGO A TARJETA DE CRÉDITO ---
        [HttpGet]
        public IActionResult CreditCardPayment() => View(new CreditCardPaymentViewModel());

        [HttpPost]
        public async Task<IActionResult> CreditCardPayment(CreditCardPaymentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var command = mapper.Map<PayCreditCardCommand>(model);
                command.CashierId = GetCashierId();
                var response = await mediator.Send(command);

                if (!response.Approved)
                {
                    ModelState.AddModelError(string.Empty, response.RejectionReason);
                    return View(model);
                }

                TempData["SuccessMessage"] = string.IsNullOrEmpty(response.WarningMessage) ? "Pago a tarjeta realizado correctamente." : response.WarningMessage;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado: " + ex.Message);
                return View(model);
            }
        }

        // --- PAGO A PRÉSTAMO ---
        [HttpGet]
        public IActionResult LoanPayment() => View(new LoanPaymentViewModel());

        [HttpPost]
        public async Task<IActionResult> LoanPayment(LoanPaymentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var command = mapper.Map<PayLoanCommand>(model);
                command.CashierId = GetCashierId();
                var response = await mediator.Send(command);

                if (!response.Approved)
                {
                    ModelState.AddModelError(string.Empty, response.RejectionReason);
                    return View(model);
                }

                TempData["SuccessMessage"] = string.IsNullOrEmpty(response.WarningMessage) ? "Pago a préstamo realizado correctamente." : response.WarningMessage;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado: " + ex.Message);
                return View(model);
            }
        }

        // --- TRANSFERENCIA A TERCEROS ---
        [HttpGet]
        public IActionResult ThirdPartyTransfer() => View(new ThirdTransferViewModel());

        [HttpPost]
        public async Task<IActionResult> ThirdPartyTransfer(ThirdTransferViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var command = mapper.Map<TransferCommand>(model);
                command.CashierId = GetCashierId();
                var response = await mediator.Send(command);

                if (!response.Approved)
                {
                    ModelState.AddModelError(string.Empty, response.RejectionReason);
                    return View(model);
                }

                TempData["SuccessMessage"] = string.IsNullOrEmpty(response.WarningMessage) ? "Transferencia realizada correctamente." : response.WarningMessage;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado: " + ex.Message);
                return View(model);
            }
        }
   

        [HttpGet]
        public async Task<IActionResult> PreviewAccount(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                return Json(new { success = false, message = "Debe ingresar un número de cuenta." });

            try
            {
                var preview = await cashierService.GetAccountPreviewAsync(accountNumber);
                return Json(new
                {
                    success = true,
                    holderName = preview.AccountHolderFullName,
                    status = preview.Status
                });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PreviewCreditCard(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return Json(new { success = false, message = "Debe ingresar un número de tarjeta." });

            try
            {
                var preview = await cashierService.GetCreditCardPreviewAsync(cardNumber);
                return Json(new
                {
                    success = true,
                    maskedCardNumber = preview.MaskedCardNumber,
                    holderName = preview.ClientFullName,
                    currentDebt = preview.CurrentDebt,
                    status = preview.Status
                });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PreviewLoan(string loanNumber)
        {
            if (string.IsNullOrWhiteSpace(loanNumber))
                return Json(new { success = false, message = "Debe ingresar un número de préstamo." });

            try
            {
                var preview = await cashierService.GetLoanPreviewAsync(loanNumber);
                return Json(new
                {
                    success = true,
                    loanNumber = preview.LoanNumber,
                    holderName = preview.ClientFullName,
                    remainingBalance = preview.RemainingBalance,
                    status = preview.Status
                });
            }
            catch (KeyNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int GetCashierId()
        {
            var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claimValue, out int cashierId) ? cashierId : 0;
        }
    }
}