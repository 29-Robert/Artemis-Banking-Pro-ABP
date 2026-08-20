using ArtemisBankingPro.Application.Features.Admin.Queries;
using ArtemisBankingPro.Application.Features.Users.Commands.CreateUser;
using ArtemisBankingPro.Application.Features.Users.Commands.ToggleUserStatus;
using ArtemisBankingPro.Application.Features.Users.Commands.UpdateUser;
using ArtemisBankingPro.Application.Features.Users.Queries;
using ArtemisBankingPro.Application.Features.Users.Queries.GetAllUsers;
using ArtemisBankingPro.WebApp.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController(IMediator mediator) : Controller
    {
        // GET: /Admin/Index -> Dashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var indicators = await mediator.Send(new GetAdminHomeDataQuery());
            return View(indicators);
        }

        // GET: /Admin/Users -> Listado de usuarios (antes era Index)
        [HttpGet]
        public async Task<IActionResult> Users(string roleFilter, int page = 1)
        {
            var query = new GetAllUsersQuery
            {
                RoleFilter = roleFilter,
                Page = page,
                PageSize = 20
            };
            var result = await mediator.Send(query);

            ViewBag.RoleFilter = roleFilter;
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;

            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int userId)
        {
            try
            {
                var currentUserIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(currentUserIdClaim, out int currentUserId) && currentUserId == userId)
                {
                    TempData["ErrorMessage"] = "No puede cambiar su propio estado.";
                    return RedirectToAction(nameof(Users));
                }

                var command = new ToggleUserStatusCommand { UserId = userId };
                await mediator.Send(command);
                TempData["SuccessMessage"] = "El estado del usuario ha sido actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SaveUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveUserViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (vm.InitialAmount < 0)
            {
                ModelState.AddModelError("InitialAmount", "El monto inicial no puede ser negativo.");
                return View(vm);
            }

            try
            {
                var command = new CreateUserCommand
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    Cedula = vm.Cedula,
                    Email = vm.Email,
                    Username = vm.Username,
                    Password = vm.Password!,
                    RoleId = vm.RoleId,
                    InitialAmount = vm.InitialAmount
                };

                await mediator.Send(command);
                TempData["SuccessMessage"] = "Usuario creado y notificación enviada correctamente.";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var query = new GetUserByIdQuery { Id = id };
            var userDto = await mediator.Send(query);

            if (userDto == null) return NotFound();

            var vm = new SaveUserViewModel
            {
                Id = userDto.Id,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Cedula = userDto.Cedula,
                Email = userDto.Email,
                Username = userDto.Username,
                RoleId = userDto.RoleId
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveUserViewModel vm)
        {
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid) return View(vm);

            if (vm.AdditionalAmount < 0)
            {
                ModelState.AddModelError("AdditionalAmount", "El monto adicional no puede ser negativo.");
                return View(vm);
            }

            try
            {
                var command = new UpdateUserCommand
                {
                    Id = vm.Id!.Value,
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    Cedula = vm.Cedula,
                    Email = vm.Email,
                    Username = vm.Username,
                    Password = vm.Password,
                    AdditionalAmount = vm.AdditionalAmount
                };

                await mediator.Send(command);
                TempData["SuccessMessage"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }
    }
}