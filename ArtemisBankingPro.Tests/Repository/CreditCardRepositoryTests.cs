using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Persistence.Contexts;
using ArtemisBankingPro.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArtemisBankingPro.Tests.Repositories
{
  
    public class CreditCardRepositoryTests
    {
        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetByCardNumberAsync_ConNumeroExistente_DeberiaDevolverLaTarjeta()
        {
            await using var context = CreateContext();
            context.CreditCards.Add(new CreditCard
            {
                ClientId = 1,
                CardNumber = "1234567890123456",
                Status = "Activa",
                AdminId = 1,
                CreditLimit = 10000m
            });
            await context.SaveChangesAsync();

            var repository = new CreditCardRepository(context);

            var result = await repository.GetByCardNumberAsync("1234567890123456");

            Assert.NotNull(result);
            Assert.Equal(10000m, result.CreditLimit);
        }

        [Fact]
        public async Task GetByCardNumberAsync_ConNumeroInexistente_DeberiaDevolverNull()
        {
            await using var context = CreateContext();
            var repository = new CreditCardRepository(context);

            var result = await repository.GetByCardNumberAsync("0000000000000000");

            Assert.Null(result);
        }

        [Fact]
        public async Task HasActiveCreditCardAsync_ConTarjetaActiva_DeberiaDevolverTrue()
        {
            await using var context = CreateContext();
            context.CreditCards.Add(new CreditCard { ClientId = 1, CardNumber = "1111222233334444", Status = "Activa", AdminId = 1 });
            await context.SaveChangesAsync();

            var repository = new CreditCardRepository(context);

            Assert.True(await repository.HasActiveCreditCardAsync(1));
        }

        [Fact]
        public async Task HasActiveCreditCardAsync_ConSoloTarjetasCanceladas_DeberiaDevolverFalse()
        {
            await using var context = CreateContext();
            context.CreditCards.Add(new CreditCard { ClientId = 1, CardNumber = "5555666677778888", Status = "Cancelada", AdminId = 1 });
            await context.SaveChangesAsync();

            var repository = new CreditCardRepository(context);

            Assert.False(await repository.HasActiveCreditCardAsync(1));
        }

        [Fact]
        public async Task GetTotalActiveDebtByClientAsync_DeberiaSumarSoloTarjetasActivasDelCliente()
        {
            await using var context = CreateContext();

            context.CreditCards.Add(new CreditCard { ClientId = 1, CardNumber = "1000000000000001", Status = "Activa", AdminId = 1, CurrentDebt = 500m });
            context.CreditCards.Add(new CreditCard { ClientId = 1, CardNumber = "1000000000000002", Status = "Activa", AdminId = 1, CurrentDebt = 300m });
            context.CreditCards.Add(new CreditCard { ClientId = 1, CardNumber = "1000000000000003", Status = "Cancelada", AdminId = 1, CurrentDebt = 0m });
            context.CreditCards.Add(new CreditCard { ClientId = 2, CardNumber = "1000000000000004", Status = "Activa", AdminId = 1, CurrentDebt = 9999m }); // otro cliente
            await context.SaveChangesAsync();

            var repository = new CreditCardRepository(context);

            var total = await repository.GetTotalActiveDebtByClientAsync(1);

            // Solo las 2 tarjetas activas del cliente 1: 500 + 300 = 800.
            // No debe incluir la cancelada (aunque tuviera deuda) ni la del cliente 2.
            Assert.Equal(800m, total);
        }

        [Fact]
        public async Task GetTotalActiveDebtSystemWideAsync_DeberiaSumarTodasLasTarjetasActivasDeTodosLosClientes()
        {
            await using var context = CreateContext();

            context.CreditCards.Add(new CreditCard { ClientId = 1, CardNumber = "2000000000000001", Status = "Activa", AdminId = 1, CurrentDebt = 100m });
            context.CreditCards.Add(new CreditCard { ClientId = 2, CardNumber = "2000000000000002", Status = "Activa", AdminId = 1, CurrentDebt = 200m });
            context.CreditCards.Add(new CreditCard { ClientId = 3, CardNumber = "2000000000000003", Status = "Cancelada", AdminId = 1, CurrentDebt = 5000m });
            await context.SaveChangesAsync();

            var repository = new CreditCardRepository(context);

            var total = await repository.GetTotalActiveDebtSystemWideAsync();

            Assert.Equal(300m, total); // 100 + 200, ignora la cancelada aunque tenga más deuda
        }

        [Fact]
        public async Task SearchAsync_PorDefecto_DeberiaMostrarSoloActivasOrdenadasPorFechaDesc()
        {
            await using var context = CreateContext();

            context.CreditCards.Add(new CreditCard { ClientId = 1, CardNumber = "3000000000000001", Status = "Activa", AdminId = 1, CreatedAt = DateTime.UtcNow.AddDays(-2) });
            context.CreditCards.Add(new CreditCard { ClientId = 1, CardNumber = "3000000000000002", Status = "Activa", AdminId = 1, CreatedAt = DateTime.UtcNow });
            context.CreditCards.Add(new CreditCard { ClientId = 1, CardNumber = "3000000000000003", Status = "Cancelada", AdminId = 1, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var repository = new CreditCardRepository(context);

            var (items, totalCount) = await repository.SearchAsync(cedula: null, status: null, pageNumber: 1, pageSize: 20);

            Assert.Equal(2, totalCount); // la cancelada no debe aparecer por defecto
            Assert.Equal("3000000000000002", items.First().CardNumber); // la más reciente primero
        }

        [Fact]
        public async Task SearchAsync_ConEstadoTodas_DeberiaIncluirActivasYCanceladas()
        {
            await using var context = CreateContext();

            context.CreditCards.Add(new CreditCard { ClientId = 1, CardNumber = "4000000000000001", Status = "Activa", AdminId = 1 });
            context.CreditCards.Add(new CreditCard { ClientId = 1, CardNumber = "4000000000000002", Status = "Cancelada", AdminId = 1 });
            await context.SaveChangesAsync();

            var repository = new CreditCardRepository(context);

            var (items, totalCount) = await repository.SearchAsync(cedula: null, status: "Todas", pageNumber: 1, pageSize: 20);

            Assert.Equal(2, totalCount);
        }

        [Fact]
        public async Task SearchAsync_ConPageSizeMenorQueElTotal_DeberiaPaginarCorrectamente()
        {
            await using var context = CreateContext();

            for (var i = 1; i <= 25; i++)
            {
                context.CreditCards.Add(new CreditCard
                {
                    ClientId = 1,
                    CardNumber = $"500000000000{i:D4}",
                    Status = "Activa",
                    AdminId = 1,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i)
                });
            }
            await context.SaveChangesAsync();

            var repository = new CreditCardRepository(context);

            var (items, totalCount) = await repository.SearchAsync(cedula: null, status: null, pageNumber: 1, pageSize: 20);

            Assert.Equal(25, totalCount);
            Assert.Equal(20, items.Count);
        }
    }
}
