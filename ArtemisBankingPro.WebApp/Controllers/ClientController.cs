using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.DTOs.Beneficiaries;
using ArtemisBankingPro.Application.DTOs.Transactions;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApp.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClientController : Controller
    {
        private readonly ISavingsAccountService _accountService;
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly IBeneficiaryRepository _beneficiaryRepository;
        private readonly ITransactionService _transactionService;
        private readonly ICreditCardRepository _cardRepository;
        private readonly ILoanRepository _loanRepository;

        public ClientController(
            ISavingsAccountService accountService,
            ISavingsAccountRepository accountRepository,
            IBeneficiaryService beneficiaryService,
            IBeneficiaryRepository beneficiaryRepository,
            ITransactionService transactionService,
            ICreditCardRepository cardRepository,
            ILoanRepository loanRepository)
        {
            _accountService = accountService;
            _accountRepository = accountRepository;
            _beneficiaryService = beneficiaryService;
            _beneficiaryRepository = beneficiaryRepository;
            _transactionService = transactionService;
            _cardRepository = cardRepository;
            _loanRepository = loanRepository;
        }

        private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _accountService.GetClientHomeDataAsync(GetCurrentUserId());
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
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null || account.UserId != int.Parse(GetCurrentUserId())) return NotFound();

            int pageSize = 10;
            var history = await _accountService.GetTransactionHistoryAsync(accountNumber, page, pageSize);

            ViewBag.Account = account;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            return View(history);
        }

        public async Task<IActionResult> LoanDetails(string loanNumber)
        {
            var loan = await _loanRepository.GetByLoanNumberAsync(loanNumber);
            if (loan == null) return NotFound();
            return View(loan);
        }

        public async Task<IActionResult> CardDetails(string cardNumber)
        {
            var card = await _cardRepository.GetByCardNumberAsync(cardNumber);
            if (card == null) return NotFound();
            return View(card);
        }

        [HttpGet]
        public async Task<IActionResult> TransferOwn()
        {
            var data = await _accountService.GetClientHomeDataAsync(GetCurrentUserId());
            ViewBag.Accounts = data.Accounts;
            return View(new OwnAccountTransferDto());
        }

        [HttpPost]
        public async Task<IActionResult> TransferOwn(OwnAccountTransferDto dto)
        {
            if (!ModelState.IsValid)
            {
                var data = await _accountService.GetClientHomeDataAsync(GetCurrentUserId());
                ViewBag.Accounts = data.Accounts;
                return View(dto);
            }

            try
            {
                var result = await _transactionService.OwnAccountTransferAsync(dto, GetCurrentUserId());
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            var homeData = await _accountService.GetClientHomeDataAsync(GetCurrentUserId());
            ViewBag.Accounts = homeData.Accounts;
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> TransferExpress()
        {
            var data = await _accountService.GetClientHomeDataAsync(GetCurrentUserId());
            ViewBag.Accounts = data.Accounts;
            return View(new ExpressTransactionDto());
        }

        [HttpPost]
        public async Task<IActionResult> TransferExpress(ExpressTransactionDto dto)
        {
            if (!ModelState.IsValid)
            {
                var data = await _accountService.GetClientHomeDataAsync(GetCurrentUserId());
                ViewBag.Accounts = data.Accounts;
                return View(dto);
            }

            try
            {
                await _transactionService.ExpressTransactionAsync(dto);
                TempData["SuccessMessage"] = "Transferencia Express realizada con éxito.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            var homeData = await _accountService.GetClientHomeDataAsync(GetCurrentUserId());
            ViewBag.Accounts = homeData.Accounts;
            return View(dto);
        }

        public async Task<IActionResult> Beneficiaries()
        {
            int clientId = int.Parse(GetCurrentUserId());
            var beneficiaries = await _beneficiaryRepository.GetByClientAsync(clientId);
            return View(beneficiaries);
        }

        [HttpGet]
        public IActionResult AddBeneficiary()
        {
            return View(new CreateBeneficiaryDto());
        }

        [HttpPost]
        public async Task<IActionResult> AddBeneficiary(CreateBeneficiaryDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                int clientId = int.Parse(GetCurrentUserId());
                await _beneficiaryService.AddBeneficiaryAsync(clientId, dto);
                TempData["SuccessMessage"] = "Beneficiario agregado exitosamente.";
                return RedirectToAction("Beneficiaries");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveBeneficiary(int id)
        {
            try
            {
                await _beneficiaryService.RemoveBeneficiaryAsync(id);
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
            var data = await _accountService.GetClientHomeDataAsync(GetCurrentUserId());
            ViewBag.Accounts = data.Accounts;
            ViewBag.DestAccount = destAccount;
            ViewBag.Alias = alias;
            
            var model = new ExpressTransactionDto { DestinationAccountNumber = destAccount };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TransferBeneficiary(ExpressTransactionDto dto)
        {
            if (!ModelState.IsValid)
            {
                var data = await _accountService.GetClientHomeDataAsync(GetCurrentUserId());
                ViewBag.Accounts = data.Accounts;
                return View(dto);
            }

            try
            {
                await _beneficiaryService.TransferToBeneficiaryAsync(dto);
                TempData["SuccessMessage"] = "Transferencia a beneficiario realizada con éxito.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            var homeData = await _accountService.GetClientHomeDataAsync(GetCurrentUserId());
            ViewBag.Accounts = homeData.Accounts;
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> PayCreditCard(string cardNumber)
        {
            var data = await _accountService.GetClientHomeDataAsync(GetCurrentUserId());
            ViewBag.Accounts = data.Accounts;
            ViewBag.CardNumber = cardNumber;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PayCreditCard(string sourceAccountNumber, string cardNumber, decimal amount)
        {
            try
            {
                await _accountService.ProcessCreditCardPaymentOwnAccountAsync(sourceAccountNumber, cardNumber, amount, GetCurrentUserId());
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
            var data = await _accountService.GetClientHomeDataAsync(GetCurrentUserId());
            ViewBag.Accounts = data.Accounts;
            ViewBag.LoanNumber = loanNumber;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PayLoan(string sourceAccountNumber, string loanNumber, decimal amount)
        {
            try
            {
                await _accountService.ProcessLoanPaymentOwnAccountAsync(sourceAccountNumber, loanNumber, amount, GetCurrentUserId());
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
            var data = await _accountService.GetClientHomeDataAsync(GetCurrentUserId());
            ViewBag.Accounts = data.Accounts;
            ViewBag.CardNumber = cardNumber;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CashAdvance(string sourceAccountNumber, string cardNumber, decimal amount)
        {
            try
            {
                await _accountService.ProcessCashAdvanceAsync(sourceAccountNumber, cardNumber, amount, GetCurrentUserId());
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
