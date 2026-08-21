using ArtemisBankingPro.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TestToggle {
    class Program {
        static void Main() {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ArtemisBankingProDb;Trusted_Connection=True;MultipleActiveResultSets=true")
                .Options;
            using var db = new ApplicationDbContext(options);
            var user = db.Users.FirstOrDefault(u => !u.IsActive);
            if (user != null) {
                System.Console.WriteLine($"Found inactive user: {user.Username}");
            } else {
                System.Console.WriteLine("No inactive user found.");
            }
        }
    }
}
