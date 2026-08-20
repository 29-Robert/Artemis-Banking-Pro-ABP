using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Features.CreditCard.Commands;
using ArtemisBankingPro.Application.Features.CreditCard.Queries;
using ArtemisBankingPro.WebApp.ViewModels;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBankingPro.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CreditCardController(IMediator mediator, IMapper mapper) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(string? cedula, string? status, int page = 1)
        {
            ViewBag.Cedula = cedula;
            ViewBag.Status = status;

            try
            {
                var query = new GetAllCreditCardsQuery { Cedula = cedula, Status = status, PageNumber = page, PageSize = 20 };
                var result = await mediator.Send(query);

                ViewBag.CurrentPage = result.PageNumber;
                ViewBag.TotalPages = result.TotalPages;

                return View(result.Items);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                ViewBag.CurrentPage = 1;
                ViewBag.TotalPages = 1;

                return View(new List<CreditCardResponseDto>());
            }


        }


            [HttpPost]
            public IActionResult SelectClient(SelectClientViewModel model)
            {
                if (!ModelState.IsValid)
                    return RedirectToAction(nameof(SelectClient), new { cedula = model.Cedula });

                return RedirectToAction(nameof(Assign), new { clientId = model.SelectedClientId });
            }



            [HttpGet]
            public IActionResult Assign(int clientId)
            {
                return View(new AssignCreditCardViewModel { ClientId = clientId });
            }

            [HttpPost]
            public async Task<IActionResult> Assign(AssignCreditCardViewModel model)
            {
                if (!ModelState.IsValid) return View(model);

                try
                {
                    var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    int.TryParse(adminIdClaim, out var adminId);

                    var command = mapper.Map<AssignCreditCardCommand>(model);
                    command.AdminId = adminId;

                    await mediator.Send(command);
                    TempData["SuccessMessage"] = "Tarjeta asignada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }
            }

            [HttpGet]
            public async Task<IActionResult> Details(int id)
            {
                var result = await mediator.Send(new GetCreditCardByIdQuery { Id = id });
                if (result == null) return NotFound();

                return View(result);
            }

            [HttpGet]
            public async Task<IActionResult> EditLimit(int id)
            {
                var card = await mediator.Send(new GetCreditCardByIdQuery { Id = id });
                if (card == null) return NotFound();

                var model = new EditCreditLimitViewModel
                {
                    CardId = id,
                    MaskedCardNumber = card.MaskedCardNumber,
                    NewCreditLimit = card.CreditLimit
                };

                return View(model);
            }

            [HttpPost]
            public async Task<IActionResult> EditLimit(int id, EditCreditLimitViewModel model)
            {
                if (!ModelState.IsValid) return View(model);

                try
                {
                    await mediator.Send(new UpdateCreditLimitCommand { CardId = id, NewCreditLimit = model.NewCreditLimit });
                    TempData["SuccessMessage"] = "Límite actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    model.CardId = id;
                    return View(model);
                }
            }

            [HttpPost]
            public async Task<IActionResult> Cancel(int id)
            {
                try
                {
                    await mediator.Send(new CancelCreditCardCommand { CardId = id });
                    TempData["SuccessMessage"] = "Tarjeta cancelada correctamente.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                return RedirectToAction(nameof(Index));
            }

        [HttpGet]
        public async Task<IActionResult> SelectClient(string? cedula, int page = 1)
        {
            var result = await mediator.Send(new GetEligibleCreditCardClientsQuery { Cedula = cedula, PageNumber = page, PageSize = 20 });

            ViewBag.Cedula = cedula;
            ViewBag.CurrentPage = result.Clients.PageNumber;
            ViewBag.TotalPages = result.Clients.TotalPages;

            return View(result);
        }
    }
    }
