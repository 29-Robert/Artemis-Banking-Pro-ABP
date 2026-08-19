using ArtemisBankingPro.Application.Features.Accounts.Commands;
using ArtemisBankingPro.Application.Features.Accounts.Queries;
using ArtemisBankingPro.Application.Features.Users.Queries;
using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.WebApp.ViewModels;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SavingsAccountController(IMediator mediator, IMapper mapper) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, AccountStatus? status = AccountStatus.Activa, AccountType? type = null, string cedula = "")
        {
            int pageSize = 20;

            if (!string.IsNullOrWhiteSpace(cedula))
            {
                var client = await mediator.Send(new GetUserByCedulaQuery { Cedula = cedula });
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
                    return View(new List<SavingsAccountListItemDto>());
                }
            }

            var result = await mediator.Send(new GetAccountsQuery
            {
                Page = page,
                PageSize = pageSize,
                Status = status,
                Type = type,
                Cedula = cedula
            });

            if (!string.IsNullOrWhiteSpace(cedula) && result.TotalCount == 0)
            {
                ViewBag.Message = "Este cliente no tiene cuentas de ahorro registradas.";
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalCount = result.TotalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.StatusFilter = status;
            ViewBag.TypeFilter = type;
            ViewBag.CedulaFilter = cedula;
            ViewBag.TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize);

            var accounts = result.Data;
            var dtos = mapper.Map<IEnumerable<SavingsAccountListItemDto>>(accounts);
            return View(dtos);
        }

        [HttpGet]
        public async Task<IActionResult> SelectClient(string searchCedula = "")
        {
            var clientList = await mediator.Send(new GetActiveClientsWithDebtQuery { SearchCedula = searchCedula });
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

            var client = await mediator.Send(new GetUserByIdQuery { Id = selectedClientId.Value });
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

            var principalAccount = await mediator.Send(new GetPrincipalAccountByClientIdQuery { ClientId = client.Id });
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
            var client = await mediator.Send(new GetUserByIdQuery { Id = clientId });
            if (client == null) return RedirectToAction("SelectClient");

            ViewBag.Client = client;
            var model = new CreateSecondaryAccountViewModel
            {
                ClientCedula = client.Cedula,
                InitialBalance = 0.00m
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfigureSecondary(CreateSecondaryAccountViewModel model, int clientId)
        {
            var client = await mediator.Send(new GetUserByIdQuery { Id = clientId });
            ViewBag.Client = client;

            if (client == null) return RedirectToAction("SelectClient");

            if (!ModelState.IsValid) return View(model);

            try
            {
                var command = mapper.Map<CreateSecondaryAccountCommand>(model);
                await mediator.Send(command);
                TempData["SuccessMessage"] = "Cuenta secundaria creada exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string accountNumber, int page = 1)
        {
            var account = await mediator.Send(new GetAccountByAccountNumberQuery { AccountNumber = accountNumber });
            if (account == null) return NotFound();

            int pageSize = 20;
            var history = await mediator.Send(new GetStatementQuery
            {
                AccountNumber = accountNumber,
                Page = page,
                PageSize = pageSize
            });

            var accountDto = mapper.Map<SavingsAccountListItemDto>(account);
            ViewBag.Account = accountDto;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(history);
        }

        [HttpGet]
        public async Task<IActionResult> CancelConfirm(string accountNumber)
        {
            var account = await mediator.Send(new GetAccountByAccountNumberQuery { AccountNumber = accountNumber });
            if (account == null) return NotFound();

            var accountDto = mapper.Map<SavingsAccountListItemDto>(account);
            return View(accountDto);
        }

        [HttpPost]
        public async Task<IActionResult> CancelConfirmed(string accountNumber)
        {
            try
            {
                var command = new CancelSecondaryAccountCommand { AccountNumber = accountNumber };
                await mediator.Send(command);
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