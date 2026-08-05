using ArtemisBankingPro.Application.Features.Users.Commands.ToggleUserStatus;
using ArtemisBankingPro.Application.Features.Users.Queries.GetAllUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController(IMediator mediator) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var query = new GetAllUsersQuery();
            var users = await mediator.Send(query);
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int userId)
        {
            try
            {
                var command = new ToggleUserStatusCommand { UserId = userId };
                await mediator.Send(command);
                TempData["SuccessMessage"] = "El estado del usuario ha sido actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}