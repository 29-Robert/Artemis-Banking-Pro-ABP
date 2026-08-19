using ArtemisBankingPro.Application.DTOs;
using ArtemisBankingPro.Application.Features.Accounts.Commands;
using ArtemisBankingPro.Application.Features.Accounts.Queries;
using ArtemisBankingPro.Application.Features.CreditCard.Queries;
using ArtemisBankingPro.Application.Features.Loans.Queries;
using ArtemisBankingPro.Application.Features.Users.Queries;
using ArtemisBankingPro.WebApp.ViewModels;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApp.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClientController(IMediator mediator, IMapper mapper) : Controller
    {
        private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Login", "Account");
            }
        }

        public async Task<IActionResult> AccountDetails(string accountNumber, int page = 1)
        {
            var account = await mediator.Send(new GetAccountByAccountNumberQuery { AccountNumber = accountNumber });
            if (account == null || account.UserId != int.Parse(GetCurrentUserId())) return NotFound();

            int pageSize = 10;
            var history = await mediator.Send(new GetStatementQuery
            {
                AccountNumber = accountNumber,
                Page = page,
                PageSize = pageSize
            });

            var accountDto = mapper.Map<Application.DTOs.Account.SavingsAccountListItemDto>(account);
            ViewBag.Account = accountDto;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            return View(history);
        }

        public async Task<IActionResult> LoanDetails(string loanNumber)
        {
            var loan = await mediator.Send(new GetLoanByNumberQuery { LoanNumber = loanNumber });
            if (loan == null) return NotFound();
            return View(loan);
        }

        public async Task<IActionResult> CardDetails(string cardNumber)
        {
            var card = await mediator.Send(new GetCreditCardByNumberQuery { CardNumber = cardNumber });
            if (card == null) return NotFound();
            return View(card);
        }

        [HttpGet]
        public async Task<IActionResult> TransferOwn()
        {
            var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = data.Accounts;
            return View(new OwnAccountTransferViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> TransferOwn(OwnAccountTransferViewModel model)
        {
            if (model.SourceAccountNumber == model.DestinationAccountNumber)
            {
                ModelState.AddModelError(string.Empty, "La cuenta de origen y destino no pueden ser la misma.");
            }

            if (!ModelState.IsValid)
            {
                var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
                ViewBag.Accounts = data.Accounts;
                return View(model);
            }

            return RedirectToAction("ConfirmTransferOwn", model);
        }

        [HttpGet]
        public IActionResult ConfirmTransferOwn(OwnAccountTransferViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteTransferOwn(OwnAccountTransferViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
                ViewBag.Accounts = data.Accounts;
                return View("TransferOwn", model);
            }

            try
            {
                var command = mapper.Map<TransferCommand>(model);
                command.ClientId = GetCurrentUserId();

                var result = await mediator.Send(command);
                if (result.Approved)
                {
                    TempData["SuccessMessage"] = "Transferencia entre cuentas propias realizada con éxito.";
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, result.RejectionReason ?? "No se pudo realizar la transferencia.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            var homeData = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = homeData.Accounts;
            return View("TransferOwn", model);
        }

        [HttpGet]
        public async Task<IActionResult> TransferExpress()
        {
            var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = data.Accounts;
            return View(new ExpressTransactionViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> TransferExpress(ExpressTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
                ViewBag.Accounts = data.Accounts;
                return View(model);
            }

            return RedirectToAction("ConfirmTransferExpress", model);
        }

        [HttpGet]
        public IActionResult ConfirmTransferExpress(ExpressTransactionViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteTransferExpress(ExpressTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
                ViewBag.Accounts = data.Accounts;
                return View("TransferExpress", model);
            }

            try
            {
                var command = mapper.Map<TransferCommand>(model);
                command.ClientId = GetCurrentUserId();
                await mediator.Send(command);
                TempData["SuccessMessage"] = "Transferencia Express realizada con éxito.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            var homeData = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = homeData.Accounts;
            return View("TransferExpress", model);
        }

        public async Task<IActionResult> Beneficiaries()
        {
            int clientId = int.Parse(GetCurrentUserId());
            var beneficiaries = await mediator.Send(new GetBeneficiariesByClientIdQuery { ClientId = clientId });
            return View(beneficiaries);
        }

        [HttpGet]
        public IActionResult AddBeneficiary()
        {
            return View(new CreateBeneficiaryViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddBeneficiary(CreateBeneficiaryViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var command = mapper.Map<CreateBeneficiaryCommand>(model);
                command.ClientId = int.Parse(GetCurrentUserId());
                await mediator.Send(command);
                TempData["SuccessMessage"] = "Beneficiario agregado exitosamente.";
                return RedirectToAction("Beneficiaries");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveBeneficiary(int id)
        {
            try
            {
                await mediator.Send(new RemoveBeneficiaryCommand { Id = id });
                TempData["SuccessMessage"] = "Beneficiario eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("Beneficiaries");
        }

        [HttpGet]
        public async Task<IActionResult> TransferBeneficiary(string destAccount, string alias)
        {
            var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = data.Accounts;
            ViewBag.DestAccount = destAccount;
            ViewBag.Alias = alias;

            var model = new ExpressTransactionViewModel { DestinationAccountNumber = destAccount };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TransferBeneficiary(ExpressTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
                ViewBag.Accounts = data.Accounts;
                return View(model);
            }

            return RedirectToAction("ConfirmTransferBeneficiary", model);
        }

        [HttpGet]
        public IActionResult ConfirmTransferBeneficiary(ExpressTransactionViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteTransferBeneficiary(ExpressTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
                ViewBag.Accounts = data.Accounts;
                return View("TransferBeneficiary", model);
            }

            try
            {
                var command = mapper.Map<TransferCommand>(model);
                command.IsBeneficiary = true;
                command.ClientId = GetCurrentUserId();
                await mediator.Send(command);
                TempData["SuccessMessage"] = "Transferencia a beneficiario realizada con éxito.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            var homeData = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = homeData.Accounts;
            return View("TransferBeneficiary", model);
        }

        [HttpGet]
        public async Task<IActionResult> PayCreditCard(string cardNumber)
        {
            var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = data.Accounts;
            var model = new CreditCardPaymentViewModel { CardNumber = cardNumber };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PayCreditCard(CreditCardPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
                ViewBag.Accounts = data.Accounts;
                return View(model);
            }

            try
            {
                var command = mapper.Map<PayCreditCardOwnAccountCommand>(model);
                command.UserId = GetCurrentUserId();
                await mediator.Send(command);
                TempData["SuccessMessage"] = "Pago de tarjeta realizado con éxito.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            var homeData = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = homeData.Accounts;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PayLoan(string loanNumber)
        {
            var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = data.Accounts;
            var model = new LoanPaymentViewModel { LoanNumber = loanNumber };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PayLoan(LoanPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
                ViewBag.Accounts = data.Accounts;
                return View(model);
            }

            try
            {
                var command = mapper.Map<PayLoanOwnAccountCommand>(model);
                command.UserId = GetCurrentUserId();
                await mediator.Send(command);
                TempData["SuccessMessage"] = "Pago de préstamo realizado con éxito.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            var homeData = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = homeData.Accounts;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CashAdvance(string cardNumber)
        {
            var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = data.Accounts;
            var model = new CashAdvanceViewModel { CardNumber = cardNumber };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CashAdvance(CashAdvanceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
                ViewBag.Accounts = data.Accounts;
                return View(model);
            }

            return RedirectToAction("ConfirmCashAdvance", model);
        }

        [HttpGet]
        public IActionResult ConfirmCashAdvance(CashAdvanceViewModel model)
        {
            ViewBag.Commission = model.Amount * 0.0625m;
            ViewBag.TotalCharge = model.Amount + (model.Amount * 0.0625m);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteCashAdvance(CashAdvanceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
                ViewBag.Accounts = data.Accounts;
                return View("CashAdvance", model);
            }

            try
            {
                var command = mapper.Map<CashAdvanceCommand>(model);
                command.UserId = GetCurrentUserId();
                await mediator.Send(command);
                TempData["SuccessMessage"] = "Avance de efectivo procesado con éxito.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            var homeData = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = homeData.Accounts;
            return View("CashAdvance", model);
        }
    }
}
