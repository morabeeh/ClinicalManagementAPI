using ClinicalManagementAPI.Models.Bookings;
using ClinicalManagementAPI.Models.Doctors;
using ClinicalManagementAPI.Models.Patients;
using ClinicalManagementAPI.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalManagementAPI.Models.Prescription
{
    public class PrescriptionDetails
    {

        [Key]
        public int PrescriptionId { get; set; }


        [StringLength(200)]
        public string? PrescriptionName { get; set; }

        [StringLength(500)]
        public string? PrescriptionDescription { get; set; }


        [StringLength(200)]
        public string? DeseaseName { get; set; }

        [StringLength(200)]
        public string? DeseaseType { get; set; }

        [StringLength(200)]
        public string? DeseaseDescription { get; set; }

        public double? MorningDosage { get; set; }

        public double? NoonDosage { get; set; }

        public double? NightDosage { get; set; }

        public double? OtherDosage { get; set; }

        [StringLength(50)]
        public string? MorningDosageTime { get; set; }

        [StringLength(50)]
        public string? NoonDosageTime { get; set; }

        [StringLength(50)]
        public string? NightDosageTime { get; set; }

        [StringLength(50)]
        public string? OtherDosageTime { get; set; }

        [StringLength(300)]
        public string? DoctorAdvices { get; set; }

        public int? PatientId { get; set; }
        [ForeignKey("PatientId")]
        public PatientDetails? PatientDetails { get; set; }

        public int ? PatientHistoryId { get; set; }
        [ForeignKey("HistoryId")]
        public PatientHistory? PatientHistory { get; set; }

        public int ? DoctorId { get; set; }
        [ForeignKey("PatientId")]
        public DoctorDetails? DoctorDetails { get; set; }

        public int ? BookingId { get; set; }
        [ForeignKey("BookingId")]
        public BookingDetails? BookingDetails { get; set; }



    }
}
