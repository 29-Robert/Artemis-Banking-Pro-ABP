using ArtemisBankingPro.Application.Features.Accounts.Commands;
using ArtemisBankingPro.Application.Features.Accounts.Queries;
using ArtemisBankingPro.WebApp.ViewModels;
using ArtemisBankingPro.WebApp.Models;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApp.Controllers
{
    [Authorize(Roles = "Cajero")]
    public class CashierController(IMediator mediator, IMapper mapper) : Controller
    {
        public async Task<IActionResult> Index()
        {
            int cashierId = GetCashierId();
            var indicators = await mediator.Send(new GetHomeIndicatorsQuery { CashierId = cashierId });
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
                var command = mapper.Map<DepositCommand>(model);
                command.CashierId = GetCashierId();

                var response = await mediator.Send(command);

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
                var command = mapper.Map<WithdrawCommand>(model);
                command.CashierId = GetCashierId();

                var response = await mediator.Send(command);

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
                var command = mapper.Map<PayCreditCardCommand>(model);
                command.CashierId = GetCashierId();

                var response = await mediator.Send(command);

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
                var command = mapper.Map<PayLoanCommand>(model);
                command.CashierId = GetCashierId();

                var response = await mediator.Send(command);

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
                var command = mapper.Map<TransferCommand>(model);
                command.CashierId = GetCashierId();

                var response = await mediator.Send(command);

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