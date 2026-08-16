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
        }
    }
}