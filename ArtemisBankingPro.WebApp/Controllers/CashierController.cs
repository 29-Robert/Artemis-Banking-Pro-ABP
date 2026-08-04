using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBankingPro.WebApp.Controllers
{
    [Authorize(Roles = "Cajero")] 
    public class CashierController : Controller
    {
        private readonly ICashierService _cashierService;

        public CashierController(ICashierService cashierService)
        {
            _cashierService = cashierService;
        }

        public IActionResult Index()
        {
           
            return View();
        }

        [HttpGet]
        public IActionResult Deposit()
        {
            return View(new DepositViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(DepositViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Obtenemos el Cajero autenticado de la sesión (Identity Cookies)
                var cashierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Procesamos el depósito en el servicio de negocio
                await _cashierService.ProcessDepositAsync(model.TargetAccountNumber, model.Amount, cashierId);

                TempData["SuccessMessage"] = "El depósito fue realizado correctamente.";
                return RedirectToAction("Index"); 
            }
            catch (Exception ex)
            {
                
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
    }
}