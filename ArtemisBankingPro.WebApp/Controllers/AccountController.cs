using ArtemisBankingPro.Application.Features.Users.Commands.ActivateUser;
using ArtemisBankingPro.Application.Features.Users.Commands.CreateUser;
using ArtemisBankingPro.Application.Features.Users.Commands.ForgotPassword;
using ArtemisBankingPro.Application.Features.Users.Commands.ResetPassword;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using ArtemisBankingPro.WebApp.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBankingPro.WebApp.Controllers
{
    public class AccountController(IGenericRepository<User> userRepository, IMediator mediator) : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var allUsers = await userRepository.GetAllAsync();
            var user = allUsers.FirstOrDefault(u => u.Username == model.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Su cuenta se encuentra inactiva. Debe activarla antes de iniciar sesión.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),

                new Claim(ClaimTypes.Role, ObtenerNombreDeRol(user.RoleId))
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var command = new CreateUserCommand
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Cedula = model.Cedula,
                    Email = model.Email,
                    Username = model.Username,
                    Password = model.Password,
                    RoleId = model.RoleId,
                    CommerceId = null
                };

                await mediator.Send(command);

                TempData["SuccessMessage"] = "Usuario registrado con éxito. Revise su correo para la activación.";

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Activate(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "El enlace de activación es inválido o está incompleto.";
                return View();
            }

            try
            {
                var command = new ActivateUserCommand { Token = token };
                await mediator.Send(command);

                ViewBag.Message = "Su cuenta ha sido activada exitosamente. Ya puede iniciar sesión.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var command = new ForgotPasswordCommand { Email = model.Email };
            await mediator.Send(command);

            ViewBag.Message = "Si el correo existe en nuestro sistema, recibirá un enlace para restablecer su contraseña.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var command = new ResetPasswordCommand
                {
                    Token = model.Token,
                    NewPassword = model.Password
                };

                await mediator.Send(command);

                TempData["SuccessMessage"] = "Contraseña restablecida exitosamente. Ya puede iniciar sesión.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private static string ObtenerNombreDeRol(int roleId)
        {
            return roleId switch
            {
                1 => "Administrador",
                2 => "Cajero",
                3 => "Cliente",
                4 => "Comercio",
                _ => "Usuario"
            };
        }
    }
}