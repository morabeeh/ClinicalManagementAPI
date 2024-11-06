using ClinicalManagementAPI.Models.Bookings;
using ClinicalManagementAPI.Models.Doctors;
using ClinicalManagementAPI.Models.Patients;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalManagementAPI.Models.Prescription
{
    public class PrescriptionHistory
    {

        [Key]
        public int PrescriptionHistoryId { get; set; }

        public DateTime? PrescribedDate { get; set; }

        public string? PrescribedDoctorName { get; set; }

        public int PrescriptionId { get; set; }
        [ForeignKey("PrescriptionId")]
        public PrescriptionDetails? PrescriptionDetails { get; set; }

        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public PatientDetails? PatientDetails { get; set; }

        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public BookingDetails? BookingDetails { get; set; }

        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public DoctorDetails? DoctorDetails { get; set; }
    }
}
