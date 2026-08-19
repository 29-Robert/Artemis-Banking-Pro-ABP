using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Features.Loan.Queries;
using ArtemisBankingPro.Application.Features.Loans.Queries;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DomainLoan = ArtemisBankingPro.Domain.Entities.Loan;

namespace ArtemisBankingPro.Tests.Queries
{
    public class GetLoansQueryHandlerTests
    {
        private static DomainLoan BuildLoan(
            int id, string loanNumber, string cedula, string status, DateTime createdAt,
            List<LoanInstallment> installments)
        {
            return new DomainLoan
            {
                Id = id,
                LoanNumber = loanNumber,
                ClientId = id,
                Status = status,
                CreatedAt = createdAt,
                Client = new User { FirstName = "Cliente", LastName = loanNumber, Cedula = cedula },
                Installments = installments
            };
        }

        [Fact]
        public async Task Handle_ReturnsPagedResult_WithCorrectPaginationMetadata()
        {
            var loans = new List<DomainLoan>
            {
                BuildLoan(1, "111111111", "001", "Activo", DateTime.UtcNow, new List<LoanInstallment>()),
                BuildLoan(2, "222222222", "002", "Activo", DateTime.UtcNow, new List<LoanInstallment>()),
                BuildLoan(3, "333333333", "003", "Activo", DateTime.UtcNow, new List<LoanInstallment>())
            };

            var repo = new Mock<IGenericRepository<DomainLoan>>();
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(loans);

            var handler = new GetLoansQueryHandler(repo.Object);
            var query = new GetLoansQuery { PageNumber = 1, PageSize = 2 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.Items.Count); 
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.PageSize);
        }

        [Fact]
        public async Task Handle_FiltersByCedula()
        {
            var loans = new List<DomainLoan>
            {
                BuildLoan(1, "111111111", "00187654321", "Activo", DateTime.UtcNow, new List<LoanInstallment>()),
                BuildLoan(2, "222222222", "00299999999", "Activo", DateTime.UtcNow, new List<LoanInstallment>())
            };

            var repo = new Mock<IGenericRepository<DomainLoan>>();
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(loans);

            var handler = new GetLoansQueryHandler(repo.Object);
            var query = new GetLoansQuery { Cedula = "00187654321", PageNumber = 1, PageSize = 20 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Single(result.Items);
            Assert.Equal("111111111", result.Items[0].LoanNumber);
        }

        [Fact]
        public async Task Handle_FiltersByStatus_CaseInsensitive()
        {
            var loans = new List<DomainLoan>
            {
                BuildLoan(1, "111111111", "001", "Activo", DateTime.UtcNow, new List<LoanInstallment>()),
                BuildLoan(2, "222222222", "002", "Completado", DateTime.UtcNow, new List<LoanInstallment>())
            };

            var repo = new Mock<IGenericRepository<DomainLoan>>();
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(loans);

            var handler = new GetLoansQueryHandler(repo.Object);
            var query = new GetLoansQuery { Status = "activo", PageNumber = 1, PageSize = 20 }; // minúsculas a propósito

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Single(result.Items);
            Assert.Equal("111111111", result.Items[0].LoanNumber);
        }

        [Fact]
        public async Task Handle_MapsClientFullName_FromClientNavigation()
        {
            var loan = new DomainLoan
            {
                Id = 1,
                LoanNumber = "111111111",
                ClientId = 1,
                Status = "Activo",
                CreatedAt = DateTime.UtcNow,
                Client = new User { FirstName = "María", LastName = "Gómez", Cedula = "001" },
                Installments = new List<LoanInstallment>()
            };

            var repo = new Mock<IGenericRepository<DomainLoan>>();
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DomainLoan> { loan });

            var handler = new GetLoansQueryHandler(repo.Object);
            var query = new GetLoansQuery { PageNumber = 1, PageSize = 20 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal("María Gómez", result.Items[0].ClientFullName);
        }

        [Fact]
        public async Task Handle_SetsClientPaymentStatusAtrasado_WhenAnyInstallmentIsLate()
        {
            var loan = BuildLoan(1, "111111111", "001", "Activo", DateTime.UtcNow, new List<LoanInstallment>
            {
                new() { InstallmentNumber = 1, InstallmentAmount = 500m, PendingInstallmentAmount = 500m, PaymentStatus = "Pendiente", IsLate = true },
                new() { InstallmentNumber = 2, InstallmentAmount = 500m, PendingInstallmentAmount = 500m, PaymentStatus = "Pendiente", IsLate = false }
            });

            var repo = new Mock<IGenericRepository<DomainLoan>>();
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DomainLoan> { loan });

            var handler = new GetLoansQueryHandler(repo.Object);
            var query = new GetLoansQuery { PageNumber = 1, PageSize = 20 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal("Atrasado", result.Items[0].ClientPaymentStatus);
        }

        [Fact]
        public async Task Handle_SetsClientPaymentStatusAlDia_WhenNoInstallmentIsLate()
        {
            var loan = BuildLoan(1, "111111111", "001", "Activo", DateTime.UtcNow, new List<LoanInstallment>
            {
                new() { InstallmentNumber = 1, InstallmentAmount = 500m, PendingInstallmentAmount = 500m, PaymentStatus = "Pendiente", IsLate = false }
            });

            var repo = new Mock<IGenericRepository<DomainLoan>>();
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DomainLoan> { loan });

            var handler = new GetLoansQueryHandler(repo.Object);
            var query = new GetLoansQuery { PageNumber = 1, PageSize = 20 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal("Al Día", result.Items[0].ClientPaymentStatus);
        }

        [Fact]
        public async Task Handle_MapsAmortizationList_WithAllInstallmentFields()
        {
            var dueDate = DateTime.UtcNow.AddMonths(1);
            var loan = BuildLoan(1, "111111111", "001", "Activo", DateTime.UtcNow, new List<LoanInstallment>
            {
                new() { InstallmentNumber = 1, DueDate = dueDate, InstallmentAmount = 500m, InterestAmount = 50m, CapitalAmount = 450m, PendingInstallmentAmount = 500m, PaymentStatus = "Pendiente", IsLate = false }
            });

            var repo = new Mock<IGenericRepository<DomainLoan>>();
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DomainLoan> { loan });

            var handler = new GetLoansQueryHandler(repo.Object);
            var query = new GetLoansQuery { PageNumber = 1, PageSize = 20 };

            var result = await handler.Handle(query, CancellationToken.None);

            var installmentDto = Assert.Single(result.Items[0].Amortization);
            Assert.Equal(1, installmentDto.InstallmentNumber);
            Assert.Equal(dueDate, installmentDto.DueDate);
            Assert.Equal(500m, installmentDto.InstallmentAmount);
            Assert.Equal(450m, installmentDto.CapitalAmount);
        }

        

        [Fact]
        public async Task Handle_CountsPaidInstallmentsCorrectly()
        {
            var loan = BuildLoan(1, "111111111", "001", "Activo", DateTime.UtcNow, new List<LoanInstallment>
            {
                new() { InstallmentNumber = 1, InstallmentAmount = 500m, PendingInstallmentAmount = 0m, PaymentStatus = "Pagada" }, // Cuota pagada
                new() { InstallmentNumber = 2, InstallmentAmount = 500m, PendingInstallmentAmount = 500m, PaymentStatus = "Pendiente" }
            });

            var repo = new Mock<IGenericRepository<DomainLoan>>();
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DomainLoan> { loan });

            var handler = new GetLoansQueryHandler(repo.Object);
            var query = new GetLoansQuery { PageNumber = 1, PageSize = 20 };

            var result = await handler.Handle(query, CancellationToken.None);

          
            Assert.Equal(1, result.Items[0].PaidInstallments);
        }

        [Fact]
        public async Task Handle_CalculatesPendingAmountCorrectly_UsingPendingInstallmentAmount()
        {
            var loan = BuildLoan(1, "111111111", "001", "Activo", DateTime.UtcNow, new List<LoanInstallment>
            {
                new() { InstallmentNumber = 1, InstallmentAmount = 500m, PendingInstallmentAmount = 0m, PaymentStatus = "Pagada" },
                new() { InstallmentNumber = 2, InstallmentAmount = 500m, PendingInstallmentAmount = 500m, PaymentStatus = "Pendiente" }
            });

            var repo = new Mock<IGenericRepository<DomainLoan>>();
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DomainLoan> { loan });

            var handler = new GetLoansQueryHandler(repo.Object);
            var query = new GetLoansQuery { PageNumber = 1, PageSize = 20 };

            var result = await handler.Handle(query, CancellationToken.None);

         
            Assert.Equal(500m, result.Items[0].PendingAmount);
        }

        [Fact]
        public async Task Handle_MapsCreatedAt_FromEntityProperty()
        {
            var actualCreationDate = new DateTime(2020, 1, 1);
            var loan = BuildLoan(1, "111111111", "001", "Activo", actualCreationDate, new List<LoanInstallment>());

            var repo = new Mock<IGenericRepository<DomainLoan>>();
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DomainLoan> { loan });

            var handler = new GetLoansQueryHandler(repo.Object);
            var query = new GetLoansQuery { PageNumber = 1, PageSize = 20 };

            var result = await handler.Handle(query, CancellationToken.None);

            
            Assert.Equal(actualCreationDate, result.Items[0].CreatedAt);
        }
    }
}