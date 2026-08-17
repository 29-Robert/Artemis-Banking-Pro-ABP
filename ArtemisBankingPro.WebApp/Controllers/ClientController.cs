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

            ViewBag.Account = account;
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
            if (!ModelState.IsValid)
            {
                var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
                ViewBag.Accounts = data.Accounts;
                return View(model);
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
            return View(model);
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

            try
            {
                var command = mapper.Map<TransferCommand>(model);
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
            return View(model);
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

            try
            {
                var command = mapper.Map<TransferCommand>(model);
                command.IsBeneficiary = true;
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
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PayCreditCard(string cardNumber)
        {
            var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = data.Accounts;
            ViewBag.CardNumber = cardNumber;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PayCreditCard(string sourceAccountNumber, string cardNumber, decimal amount)
        {
            try
            {
                var command = new PayCreditCardOwnAccountCommand
                {
                    SourceAccountNumber = sourceAccountNumber,
                    CardNumber = cardNumber,
                    Amount = amount,
                    UserId = GetCurrentUserId()
                };
                await mediator.Send(command);
                TempData["SuccessMessage"] = "Pago de tarjeta realizado con éxito.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("PayCreditCard", new { cardNumber });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PayLoan(string loanNumber)
        {
            var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = data.Accounts;
            ViewBag.LoanNumber = loanNumber;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PayLoan(string sourceAccountNumber, string loanNumber, decimal amount)
        {
            try
            {
                var command = new PayLoanOwnAccountCommand
                {
                    SourceAccountNumber = sourceAccountNumber,
                    LoanNumber = loanNumber,
                    Amount = amount,
                    UserId = GetCurrentUserId()
                };
                await mediator.Send(command);
                TempData["SuccessMessage"] = "Pago de préstamo realizado con éxito.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("PayLoan", new { loanNumber });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CashAdvance(string cardNumber)
        {
            var data = await mediator.Send(new GetClientHomeDataQuery { ClientId = GetCurrentUserId() });
            ViewBag.Accounts = data.Accounts;
            ViewBag.CardNumber = cardNumber;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CashAdvance(string sourceAccountNumber, string cardNumber, decimal amount)
        {
            try
            {
                var command = new CashAdvanceCommand
                {
                    SourceAccountNumber = sourceAccountNumber,
                    CardNumber = cardNumber,
                    Amount = amount,
                    UserId = GetCurrentUserId()
                };
                await mediator.Send(command);
                TempData["SuccessMessage"] = "Avance de efectivo procesado con éxito.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("CashAdvance", new { cardNumber });
            }
        }
    }
}
