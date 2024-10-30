using ClinicalManagementAPI.Models.Users;
using Microsoft.EntityFrameworkCore;
using System;

namespace ClinicalManagementAPI.Data
{
    public class ClinicContext : DbContext
    {

        public ClinicContext(DbContextOptions<ClinicContext> options) : base(options)
        {


        }


        public DbSet<Users> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
}
