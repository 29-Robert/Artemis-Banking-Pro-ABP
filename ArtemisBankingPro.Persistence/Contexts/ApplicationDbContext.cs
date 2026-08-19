using ArtemisBankingPro.Domain.Common;
using ArtemisBankingPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBankingPro.Persistence.Contexts
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ConfirmationToken> ConfirmationTokens { get; set; }
        public DbSet<Commerce> Commerces { get; set; }
        public DbSet<SavingsAccount> SavingsAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanInstallment> LoanInstallments { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<CreditCardConsumption> CreditCardConsumptions { get; set; }
        public DbSet<Beneficiary> Beneficiaries { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Cedula).IsUnique();

            modelBuilder.Entity<Commerce>().HasIndex(c => c.RNC).IsUnique();
            modelBuilder.Entity<Commerce>().HasIndex(c => c.Email).IsUnique();
            modelBuilder.Entity<SavingsAccount>().HasIndex(s => s.AccountNumber).IsUnique();

            modelBuilder.Entity<Loan>().HasIndex(l => l.LoanNumber).IsUnique();
            modelBuilder.Entity<CreditCard>().HasIndex(cc => cc.CardNumber).IsUnique();

            modelBuilder.Entity<Commerce>().HasIndex(c => c.Email).IsUnique();
            modelBuilder.Entity<Loan>()
               .HasMany(l => l.Installments)
               .WithOne(i => i.Loan)
               .HasForeignKey(i => i.LoanId);

            modelBuilder.Entity<CreditCard>()
               .HasMany(c => c.Consumptions)
               .WithOne(c => c.CreditCard)
               .HasForeignKey(c => c.CreditCardId);

            modelBuilder.Entity<Commerce>()
                    .HasMany(c => c.Consumptions)
                    .WithOne(ccc => ccc.Commerce)
                    .HasForeignKey(ccc => ccc.CommerceId)
                    .OnDelete(DeleteBehavior.Restrict);

            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            modelBuilder.Entity<SavingsAccount>()
                .HasMany(s => s.Transactions)
                .WithOne(t => t.SavingsAccount)
                .HasForeignKey(t => t.AccountNumber)
                .HasPrincipalKey(s => s.AccountNumber)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.SavingsAccounts)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            var seedDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Administrador", CreatedAt = seedDate },
                new Role { Id = 2, Name = "Cajero", CreatedAt = seedDate },
                new Role { Id = 3, Name = "Cliente", CreatedAt = seedDate },
                new Role { Id = 4, Name = "Comercio", CreatedAt = seedDate }
            );

            // Se usa un hash estático (generado previamente para '123P@$$word!') para evitar que EF Core detecte cambios pendientes infinitamente.
            var passHash = "$2a$11$1qnRUBRJHLZJfRzSwt2iuurx7EVZyXvctT5h1.wrMvUSlgV2CWBha";

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "Admin",
                    LastName = "Defecto",
                    Cedula = "00000000001",
                    Email = "admin@artemis.com",
                    Username = "admin",
                    PasswordHash = passHash,
                    RoleId = 1,
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new User
                {
                    Id = 2,
                    FirstName = "Cajero",
                    LastName = "Defecto",
                    Cedula = "00000000002",
                    Email = "cajero@artemis.com",
                    Username = "cajero",
                    PasswordHash = passHash,
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new User
                {
                    Id = 3,
                    FirstName = "Cliente",
                    LastName = "Defecto",
                    Cedula = "00000000003",
                    Email = "cliente@artemis.com",
                    Username = "cliente",
                    PasswordHash = passHash,
                    RoleId = 3,
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new User
                {
                    Id = 4,
                    FirstName = "Comercio",
                    LastName = "Defecto",
                    Cedula = "00000000004",
                    Email = "comercio@artemis.com",
                    Username = "comercio",
                    PasswordHash = passHash,
                    RoleId = 4,
                    IsActive = true,
                    CreatedAt = seedDate
                }
            );
            modelBuilder.Entity<SavingsAccount>().HasData(
                new SavingsAccount
                {
                    Id = 1,
                    UserId = 3,
                    AccountNumber = "100200300",
                    Balance = 5000.00m,
                    IsPrincipal = true,
                    CreatedAt = seedDate
                }
            );
        }
    }
}