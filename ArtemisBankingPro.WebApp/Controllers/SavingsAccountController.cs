using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SavingsAccountController(
        ISavingsAccountService accountService,
        ISavingsAccountRepository accountRepository,
        IUserRepository userRepository,
        IGenericRepository<User> userGenericRepository,
        ILoanRepository loanRepository,
        ICreditCardRepository creditCardRepository) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, AccountStatus? status = AccountStatus.Activa, AccountType? type = null, string cedula = "")
        {
            int pageSize = 20;

            if (!string.IsNullOrWhiteSpace(cedula))
            {
                var client = await userRepository.GetByCedulaAsync(cedula);
                if (client == null)
                {
                    ViewBag.Message = "No existe un cliente registrado con esta cédula.";
                    ViewBag.CurrentPage = 1;
                    ViewBag.TotalCount = 0;
                    ViewBag.PageSize = pageSize;
                    ViewBag.StatusFilter = status;
                    ViewBag.TypeFilter = type;
                    ViewBag.CedulaFilter = cedula;
                    ViewBag.TotalPages = 0;
                    return View(new List<SavingsAccount>());
                }
            }

            var resultObj = await accountRepository.GetPagedAsync(page, pageSize, status, type, cedula);
            var data = (dynamic)resultObj;

            if (!string.IsNullOrWhiteSpace(cedula) && data.TotalCount == 0)
            {
                ViewBag.Message = "Este cliente no tiene cuentas de ahorro registradas.";
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalCount = data.TotalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.StatusFilter = status;
            ViewBag.TypeFilter = type;
            ViewBag.CedulaFilter = cedula;
            ViewBag.TotalPages = (int)Math.Ceiling((double)data.TotalCount / pageSize);

            return View(data.Data);
        }

        [HttpGet]
        public async Task<IActionResult> SelectClient(string searchCedula = "")
        {
            var allUsers = await userGenericRepository.GetAllAsync();
            var activeClients = allUsers.Where(u => u.RoleId == 3 && u.IsActive).ToList();

            if (!string.IsNullOrWhiteSpace(searchCedula))
            {
                activeClients = activeClients.Where(u => u.Cedula.Contains(searchCedula)).ToList();
            }

            var clientList = new List<dynamic>();

            foreach (var client in activeClients)
            {
                var loans = await loanRepository.GetLoansByClientAsync(client.Cedula);
                var activeLoansDebt = loans.Where(l => l.Status == "Activo" || l.Status == "Aprobado").Sum(l => l.CapitalAmount);

                var cards = await creditCardRepository.GetCardsByClientAsync(client.Cedula);
                var activeCardsDebt = cards.Where(c => c.Status == "Activa").Sum(c => c.CurrentDebt);

                decimal totalDebt = activeLoansDebt + activeCardsDebt;

                clientList.Add(new {
                    Id = client.Id,
                    Cedula = client.Cedula,
                    FullName = $"{client.FirstName} {client.LastName}",
                    Email = client.Email,
                    TotalDebt = totalDebt
                });
            }

            ViewBag.SearchCedula = searchCedula;
            return View(clientList);
        }

        [HttpPost]
        public async Task<IActionResult> ValidateSelectedClient(int? selectedClientId)
        {
            if (selectedClientId == null)
            {
                TempData["ErrorMessage"] = "Debe seleccionar un cliente para continuar.";
                return RedirectToAction("SelectClient");
            }

            var client = await userGenericRepository.GetByIdAsync(selectedClientId.Value);
            if (client == null)
            {
                TempData["ErrorMessage"] = "El cliente seleccionado no existe.";
                return RedirectToAction("SelectClient");
            }

            if (!client.IsActive)
            {
                TempData["ErrorMessage"] = "Solo se puede asignar cuentas de ahorro a clientes activos.";
                return RedirectToAction("SelectClient");
            }

            var principalAccount = await accountRepository.GetPrincipalByClientAsync(client.Id);
            if (principalAccount == null || principalAccount.Status != AccountStatus.Activa)
            {
                TempData["ErrorMessage"] = "El cliente debe tener una cuenta de ahorro principal activa antes de asignarle una cuenta secundaria.";
                return RedirectToAction("SelectClient");
            }

            return RedirectToAction("ConfigureSecondary", new { clientId = client.Id });
        }

        [HttpGet]
        public async Task<IActionResult> ConfigureSecondary(int clientId)
        {
            var client = await userGenericRepository.GetByIdAsync(clientId);
            if (client == null) return RedirectToAction("SelectClient");

            ViewBag.Client = client;
            var model = new CreateSecondaryAccountDto
            {
                ClientCedula = client.Cedula,
                InitialBalance = 0.00m
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfigureSecondary(CreateSecondaryAccountDto dto, int clientId)
        {
            var client = await userGenericRepository.GetByIdAsync(clientId);
            ViewBag.Client = client;

            if (client == null) return RedirectToAction("SelectClient");

            if (!ModelState.IsValid) return View(dto);

            if (dto.InitialBalance < 0)
            {
                ModelState.AddModelError("InitialBalance", "El balance inicial no puede ser negativo.");
                return View(dto);
            }

            try
            {
                await accountService.CreateSecondaryAccountAsync(dto);
                TempData["SuccessMessage"] = "Cuenta secundaria creada exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string accountNumber, int page = 1)
        {
            var account = await accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null) return NotFound();

            int pageSize = 20;
            var history = await accountService.GetTransactionHistoryAsync(accountNumber, page, pageSize);

            ViewBag.Account = account;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(history);
        }

        [HttpGet]
        public async Task<IActionResult> CancelConfirm(string accountNumber)
        {
            var account = await accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null) return NotFound();

            return View(account);
        }

        [HttpPost]
        public async Task<IActionResult> CancelConfirmed(string accountNumber)
        {
            try
            {
                await accountService.CancelSecondaryAccountAsync(accountNumber);
                TempData["SuccessMessage"] = $"La cuenta secundaria {accountNumber} ha sido cancelada exitosamente y los fondos han sido transferidos.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}