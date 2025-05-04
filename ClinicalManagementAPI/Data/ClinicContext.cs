using ClinicalManagementAPI.Models.Bookings;
using ClinicalManagementAPI.Models.Doctors;
using ClinicalManagementAPI.Models.Patients;
using ClinicalManagementAPI.Models.Prescription;
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

        public DbSet<UserRatings> UserRatings { get; set; }

        public DbSet<LoginAttempt> LoginAttempts { get; set; }

        //Doctors
        public DbSet<DoctorDetails> Doctors { get; set; }
        public DbSet<DepartmentDetails> Departments { get; set; }
        public DbSet<DoctorAttendance> DoctorAttendances { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }

        //Patients
        public DbSet<PatientDetails> PatientDetails { get; set; }
        public DbSet<PatientHistory> PatientHistories { get; set; }

        //Booking
        public DbSet<BookingDetails> BookingDetails { get; set; }
        public DbSet<BookingHistory> BookingHistories { get; set; }


        //Prescription
        public DbSet<PrescriptionDetails> PrescriptionDetails { get; set; }
        public DbSet<PrescriptionHistory> PrescriptionHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User and Role Mapping
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor and Department Mapping
            modelBuilder.Entity<DoctorDetails>()
                .HasOne(d => d.User)
                .WithMany(u => u.Doctors)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorDetails>()
                .HasOne(d => d.Department)
                .WithMany(d => d.Doctors)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor and Availability Mapping
            modelBuilder.Entity<DoctorAvailability>()
                .HasOne(da => da.Doctor)
                .WithMany(d => d.DoctorAvaialabilities)
                .HasForeignKey(da => da.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor and Attendance Mapping
            modelBuilder.Entity<DoctorAttendance>()
                .HasOne(da => da.Doctor)
                .WithMany(d => d.DoctorAttendances)
                .HasForeignKey(da => da.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking and Patient Mapping
            modelBuilder.Entity<BookingDetails>()
                .HasOne(b => b.PatientDetails)
                .WithMany(p => p.BookingDetails)
                .HasForeignKey(b => b.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking and Doctor Mapping
            modelBuilder.Entity<BookingDetails>()
                .HasOne(b => b.DoctorDetails)
                .WithMany(d => d.BookingDetails)
                .HasForeignKey(b => b.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient History and Patient Mapping
            modelBuilder.Entity<PatientHistory>()
                .HasOne(ph => ph.Patient)
                .WithMany(p => p.PatientHistory)
                .HasForeignKey(ph => ph.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient History and Doctor Mapping
            modelBuilder.Entity<PatientHistory>()
                .HasOne(ph => ph.Doctor)
                .WithMany()
                .HasForeignKey(ph => ph.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient History and Booking Mapping
            modelBuilder.Entity<PatientHistory>()
                .HasOne(ph => ph.BookingDetails)
                .WithMany()
                .HasForeignKey(ph => ph.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient History and Booking History Mapping
            modelBuilder.Entity<PatientHistory>()
                .HasOne(ph => ph.BookingHistory)
                .WithMany()
                .HasForeignKey(ph => ph.BookingHistoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking History Mapping
            modelBuilder.Entity<BookingHistory>()
                .HasOne(bh => bh.BookingDetails)
                .WithMany(b => b.BookingHistory)
                .HasForeignKey(bh => bh.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient History and Booking History Mapping
            modelBuilder.Entity<PrescriptionDetails>()
                .HasOne(ph => ph.PatientHistory)
                .WithMany()
                .HasForeignKey(ph => ph.PatientHistoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // PrescriptionDetails and Patient Mapping
            modelBuilder.Entity<PrescriptionDetails>()
                .HasOne(pd => pd.PatientDetails)
                .WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(pd => pd.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // PrescriptionDetails and Doctor Mapping with one ways
            modelBuilder.Entity<PrescriptionDetails>()
                        .HasOne(pd => pd.DoctorDetails)       
                        .WithMany()
                        .HasForeignKey(pd => pd.DoctorId)     
                        .OnDelete(DeleteBehavior.Restrict);

            // PrescriptionDetails and Booking Mapping
            modelBuilder.Entity<PrescriptionDetails>()
                .HasOne(pd => pd.BookingDetails)
                .WithMany(b => b.PrescriptionDetails)
                .HasForeignKey(pd => pd.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // PrescriptionHistory and PrescriptionDetails Mapping
            modelBuilder.Entity<PrescriptionHistory>()
                .HasOne(ph => ph.PrescriptionDetails)
                .WithMany()
                .HasForeignKey(ph => ph.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // PrescriptionHistory and Patient Mapping
            modelBuilder.Entity<PrescriptionHistory>()
                .HasOne(ph => ph.PatientDetails)
                .WithMany()
                .HasForeignKey(ph => ph.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // PrescriptionHistory and Booking Mapping
            modelBuilder.Entity<PrescriptionHistory>()
                .HasOne(ph => ph.BookingDetails)
                .WithMany()
                .HasForeignKey(ph => ph.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // PrescriptionHistory and Doctor Mapping
            modelBuilder.Entity<PrescriptionHistory>()
                .HasOne(ph => ph.DoctorDetails)
                .WithMany()
                .HasForeignKey(ph => ph.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);


            // UserRatings and Patient Mapping
            modelBuilder.Entity<UserRatings>()
                .HasOne(ur => ur.PatientDetails)
                .WithMany()
                .HasForeignKey(ur => ur.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserRatings and Doctor Mapping
            modelBuilder.Entity<UserRatings>()
                .HasOne(ur => ur.DoctorDetails)
                .WithMany()
                .HasForeignKey(ur => ur.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserRatings and Booking Mapping
            modelBuilder.Entity<UserRatings>()
                .HasOne(ur => ur.BookingDetails)
                .WithMany()
                .HasForeignKey(ur => ur.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

        }



    }

}
