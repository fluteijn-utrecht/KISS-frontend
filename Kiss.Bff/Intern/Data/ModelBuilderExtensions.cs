using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Kiss.Bff.Intern.Data
{
    public static class ModelBuilderExtensions
    {
        public static void SeedData<T>(this ModelBuilder modelBuilder, string filePath) where T : class
        {
            var jsonData = File.ReadAllText(filePath);
            var entities = JsonSerializer.Deserialize<List<T>>(jsonData);
            if (entities != null && entities.Count > 0)
            {
                modelBuilder.Entity<T>().HasData(entities);
            }
        }
    }
}
