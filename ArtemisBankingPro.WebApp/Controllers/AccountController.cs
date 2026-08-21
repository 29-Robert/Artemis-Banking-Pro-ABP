using ArtemisBankingPro.Application.Features.Users.Commands.ActivateUser;
using ArtemisBankingPro.Application.Features.Users.Commands.ForgotPassword;
using ArtemisBankingPro.Application.Features.Users.Commands.ResetPassword;
using ArtemisBankingPro.Application.Features.Users.Commands.RequestResendActivationEmail;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.WebApp.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBankingPro.WebApp.Controllers
{
    public class AccountController(IUserRepository userRepository, IMediator mediator) : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == "Administrador") return RedirectToAction("Index", "Admin");
                if (role == "Cajero") return RedirectToAction("Index", "Cashier");
                if (role == "Cliente") return RedirectToAction("Index", "Client");

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

            var user = await userRepository.GetByUsernameAsync(model.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Los datos de acceso son inválidos.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Su cuenta se encuentra inactiva. Debe activar su cuenta mediante el enlace enviado a su correo electrónico registrado. Si no lo recibió, utilice la opción de reenviar enlace.");
                return View(model);
            }

            if (user.RoleId == (int)Roles.Comercio)
            {
                ModelState.AddModelError(string.Empty, "Este usuario no tiene permisos para acceder a la aplicación web.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, ObtenerNombreDeRol(user.RoleId))
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return user.RoleId switch
            {
                1 => RedirectToAction("Index", "Admin"),
                2 => RedirectToAction("Index", "Cashier"),
                3 => RedirectToAction("Index", "Client"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpGet]
        public async Task<IActionResult> Activate(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "El enlace de activación no es válido.";
                return View();
            }

            try
            {
                var command = new ActivateUserCommand { Token = token };
                await mediator.Send(command);

                ViewBag.Message = "Su cuenta ha sido activada correctamente. Ya puede iniciar sesión.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult ResendActivation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendActivation(ResendActivationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var command = new RequestResendActivationEmailCommand 
            { 
                EmailOrUsername = model.EmailOrUsername,
                ActivationUrlFormat = Url.Action("Activate", "Account", new { token = "TOKENPLACEHOLDER" }, Request.Scheme)
            };

            await mediator.Send(command);

            ViewBag.Message = "Si la cuenta existe y está pendiente de activación, se ha enviado un correo con el nuevo enlace.";
            return View();
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

            var command = new ForgotPasswordCommand 
            { 
                Username = model.Username,
                ResetUrlFormat = Url.Action("ResetPassword", "Account", new { token = "TOKENPLACEHOLDER" }, Request.Scheme)
            };

            try
            {
                await mediator.Send(command);
                ViewBag.Message = "Se ha enviado un enlace de restablecimiento de contraseña al correo electrónico registrado.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "El enlace de restablecimiento no es válido.";
                return View();
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

                TempData["SuccessMessage"] = "Su contraseña ha sido restablecida correctamente. Ya puede iniciar sesión.";
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
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            ViewBag.Role = role;
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