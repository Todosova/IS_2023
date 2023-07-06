using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movies.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Tickets> TicketsDb { get; set; }
        public DbSet<Cart> CartDb { get; set; }
        public DbSet<Orders> OrdersDb { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<List<string>>().HasNoKey();
            modelBuilder.Entity<Tickets>().Ignore(t => t.GenreList);
        }
*/
    }
}