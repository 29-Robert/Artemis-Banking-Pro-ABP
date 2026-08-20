using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Persistence.Contexts;
using ArtemisBankingPro.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArtemisBankingPro.Tests.Repositories
{
    
    public class LoanRepositoryTests
    {
        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task HasActiveLoanAsync_ConPrestamoActivo_DeberiaDevolverTrue()
        {
            await using var context = CreateContext();
            context.Loans.Add(new Loan { ClientId = 1, LoanNumber = "100000001", Status = "Activo", AdminId = 1 });
            await context.SaveChangesAsync();

            var repository = new LoanRepository(context);

            var result = await repository.HasActiveLoanAsync(1);

            Assert.True(result);
        }

        [Fact]
        public async Task HasActiveLoanAsync_ConPrestamoCompletado_DeberiaDevolverFalse()
        {
            await using var context = CreateContext();
            context.Loans.Add(new Loan { ClientId = 1, LoanNumber = "100000002", Status = "Completado", AdminId = 1 });
            await context.SaveChangesAsync();

            var repository = new LoanRepository(context);

            var result = await repository.HasActiveLoanAsync(1);

            Assert.False(result);
        }

        [Fact]
        public async Task LoanNumberExistsAsync_ConNumeroExistente_DeberiaDevolverTrue()
        {
            await using var context = CreateContext();
            context.Loans.Add(new Loan { ClientId = 1, LoanNumber = "123456789", Status = "Activo", AdminId = 1 });
            await context.SaveChangesAsync();

            var repository = new LoanRepository(context);

            Assert.True(await repository.LoanNumberExistsAsync("123456789"));
            Assert.False(await repository.LoanNumberExistsAsync("999999999"));
        }

        [Fact]
        public async Task GetTotalActiveDebtByClientAsync_DeberiaSumarSoloCuotasPendientesDePrestamosActivos()
        {
            await using var context = CreateContext();

            var activeLoan = new Loan { ClientId = 1, LoanNumber = "100000003", Status = "Activo", AdminId = 1 };
            activeLoan.Installments.Add(new LoanInstallment { InstallmentNumber = 1, PendingInstallmentAmount = 500m, PaymentStatus = "Pendiente" });
            activeLoan.Installments.Add(new LoanInstallment { InstallmentNumber = 2, PendingInstallmentAmount = 500m, PaymentStatus = "Pendiente" });

            var completedLoan = new Loan { ClientId = 1, LoanNumber = "100000004", Status = "Completado", AdminId = 1 };
            completedLoan.Installments.Add(new LoanInstallment { InstallmentNumber = 1, PendingInstallmentAmount = 0m, PaymentStatus = "Pagada" });

            context.Loans.AddRange(activeLoan, completedLoan);
            await context.SaveChangesAsync();

            var repository = new LoanRepository(context);

            var total = await repository.GetTotalActiveDebtByClientAsync(1);

           
            Assert.Equal(1000m, total);
        }

        [Fact]
        public async Task SearchAsync_PorDefecto_DeberiaMostrarTodosOrdenadosPorFechaDesc()
        {
            await using var context = CreateContext();

            var client = new User { Id = 1, Cedula = "40200000001", FirstName = "John", LastName = "Doe" };
            context.Users.Add(client);

            var loan1 = new Loan { ClientId = 1, LoanNumber = "100000005", Status = "Activo", AdminId = 1 };
            var loan2 = new Loan { ClientId = 1, LoanNumber = "100000006", Status = "Activo", AdminId = 1 };
            var loan3 = new Loan { ClientId = 1, LoanNumber = "100000007", Status = "Completado", AdminId = 1 };

            context.Loans.AddRange(loan1, loan2, loan3);
            await context.SaveChangesAsync(); 

          
            loan1.CreatedAt = DateTime.UtcNow.AddDays(-2);
            loan2.CreatedAt = DateTime.UtcNow;
            loan3.CreatedAt = DateTime.UtcNow.AddDays(-1);
            await context.SaveChangesAsync();

            var repository = new LoanRepository(context);

            var (items, totalCount) = await repository.SearchAsync(cedula: null, status: null, pageNumber: 1, pageSize: 20);

            Assert.Equal(3, totalCount);
            Assert.Equal("100000006", items.First().LoanNumber);
        }

        [Fact]
        public async Task SearchAsync_ConPageSizeMenorQueElTotal_DeberiaPaginarCorrectamente()
        {
            await using var context = CreateContext();
            var client = new User { Id = 1, Cedula = "40200000001", FirstName = "John", LastName = "Doe" };
            context.Users.Add(client);

            for (var i = 1; i <= 25; i++)
            {
                context.Loans.Add(new Loan
                {
                    ClientId = 1,
                    LoanNumber = $"1000000{i:D2}",
                    Status = "Activo",
                    AdminId = 1,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i)
                });
            }
            await context.SaveChangesAsync();

            var repository = new LoanRepository(context);

            var (items, totalCount) = await repository.SearchAsync(cedula: null, status: null, pageNumber: 1, pageSize: 20);

            Assert.Equal(25, totalCount);
            Assert.Equal(20, items.Count); 
        }
    }
}

