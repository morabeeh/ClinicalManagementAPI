using ClinicalManagementAPI.Models.Doctors;
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


        public DbSet<UserDetails> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<DoctorDetails> Doctors { get; set; }
        public DbSet<DepartmentDetails> Departments { get; set; }
        public DbSet<DoctorAttendance> DoctorAttendances { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorDetails>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.Doctors)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorDetails>()
                .HasOne(ur => ur.Department)
                .WithMany(u => u.Doctors)
                .HasForeignKey(ur => ur.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorAvailability>()
                .HasOne(ur => ur.Doctor)
                .WithMany(u => u.DoctorAvaialabilities)
                .HasForeignKey(ur => ur.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorAttendance>()
                .HasOne(ur => ur.Doctor)
                .WithMany(u => u.DoctorAttendances)
                .HasForeignKey(ur => ur.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
    
}
