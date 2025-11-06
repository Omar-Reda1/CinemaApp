using CinemaApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace CinemaApp.DataAccess
{
    public class ApplicationDbContext:IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }
        public DbSet<ApplicationUserOTP> ApplicationUserOTPs { get; set; }

       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<MovieActor>()
                .HasKey(ma => new { ma.MovieId, ma.ActorId });

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieId);

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId);
             
            base.OnModelCreating(modelBuilder);
        }

    }
}
