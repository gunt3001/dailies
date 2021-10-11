using dailies.Shared;
using Microsoft.EntityFrameworkCore;
using System;

namespace dailies.Server.Models
{
    public class EntriesContext : DbContext
    {
        private static readonly string DatabaseFileName = Environment.GetEnvironmentVariable("DB_FILE") ?? "dailies.sqlite";

        public DbSet<Entry> Entries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"data Source={DatabaseFileName}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entry>()
                .HasKey(x => x.Date);
        }
    }
}
