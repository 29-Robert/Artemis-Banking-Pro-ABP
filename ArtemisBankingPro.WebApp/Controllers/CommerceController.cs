using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Features.Commerces.Commands;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.WebApp.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CommerceController(
        IHttpClientFactory httpClientFactory,
        IJwtService jwtService,
        IConfiguration configuration,
        IMapper mapper) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var client = CreateAuthorizedClient();
            try
            {
                var response = await client.GetFromJsonAsync<PagedCommerceResponseDto>($"Commerce?page={page}&limit=10");
                if (response == null)
                {
                    ViewBag.CurrentPage = 1;
                    ViewBag.TotalPages = 1;
                    return View(new List<CommerceDetailDto>());
                }

                ViewBag.CurrentPage = response.CurrentPage;
                ViewBag.TotalPages = response.TotalPages;
                return View(response.Data);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar los comercios: {ex.Message}");
                ViewBag.CurrentPage = 1;
                ViewBag.TotalPages = 1;
                return View(new List<CommerceDetailDto>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateCommerceViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCommerceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = CreateAuthorizedClient();
            var command = mapper.Map<CreateCommerceCommand>(model);

            try
            {
                var response = await client.PostAsJsonAsync("Commerce", command);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Comercio creado y afiliado correctamente.";
                    return RedirectToAction(nameof(Index));
                }

                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                ModelState.AddModelError(string.Empty, problem?.Detail ?? "Error al registrar el comercio.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error de conexión con la API: {ex.Message}");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateAuthorizedClient();
            try
            {
                var response = await client.GetAsync($"Commerce/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var commerce = await response.Content.ReadFromJsonAsync<CommerceDetailDto>();
                    if (commerce != null)
                    {
                        var model = new UpdateCommerceViewModel
                        {
                            Id = commerce.Id,
                            BusinessName = commerce.BusinessName,
                            RNC = commerce.RNC,
                            Email = commerce.Email,
                            Phone = commerce.Phone,
                            Address = commerce.Address
                        };
                        return View(model);
                    }
                }
                TempData["ErrorMessage"] = "No se pudo recuperar los datos del comercio.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al recuperar el comercio: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdateCommerceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = CreateAuthorizedClient();
            var command = mapper.Map<UpdateCommerceCommand>(model);
            command.Id = id;

            try
            {
                var response = await client.PutAsJsonAsync($"Commerce/{id}", command);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Comercio actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }

                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                ModelState.AddModelError(string.Empty, problem?.Detail ?? "Error al actualizar el comercio.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error de conexión con la API: {ex.Message}");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            var client = CreateAuthorizedClient();
            var statusDto = new UpdateCommerceStatusDto { IsActive = isActive };

            try
            {
                var response = await client.PatchAsJsonAsync($"Commerce/{id}/status", statusDto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = isActive ? "Comercio activado correctamente." : "Comercio desactivado correctamente.";
                }
                else
                {
                    var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                    TempData["ErrorMessage"] = problem?.Detail ?? "Error al cambiar el estado del comercio.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión con la API: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private HttpClient CreateAuthorizedClient()
        {
            var client = httpClientFactory.CreateClient();
            var apiBase = configuration["ApiBaseUrl"] ?? "https://localhost:7181/api/";
            client.BaseAddress = new Uri(apiBase);

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.Identity?.Name ?? "";
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "";

            int.TryParse(userIdStr, out var userId);

            var user = new User
            {
                Id = userId,
                Username = username,
                Email = email,
                RoleId = role == "Administrador" ? 1 : (role == "Cajero" ? 2 : (role == "Cliente" ? 3 : 4))
            };

            var token = jwtService.GenerateToken(user);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }
    }
}
