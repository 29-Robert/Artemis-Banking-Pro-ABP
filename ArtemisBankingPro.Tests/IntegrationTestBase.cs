using ArtemisBankingPro.Persistence.Contexts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;

namespace ArtemisBankingPro.Tests
{
    public abstract class IntegrationTestBase : IDisposable
    {
        private readonly SqliteConnection _connection;
        protected readonly ApplicationDbContext DbContext;

        protected IntegrationTestBase()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();
            }

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            DbContext = new ApplicationDbContext(options);
            DbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            DbContext.Dispose();
            _connection.Close();
            _connection.Dispose();
        }
    }
}
