using ClinicalManagementAPI.Models.Bookings;
using ClinicalManagementAPI.Models.Prescription;
using ClinicalManagementAPI.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalManagementAPI.Models.Patients
{
    public class PatientDetails
    {
        [Key]
        public int PatientId { get; set; }

        public Guid PatientGuid { get; set; } = Guid.NewGuid();
        public string ?PatientName { get; set; }

        public string ?PatientDescription { get; set; }

        public string? PatientHealthCondition { get; set; }

        //User Details
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public UserDetails ?User { get; set; }

        //Patient History
        public ICollection<PatientHistory> ?PatientHistory { get; set; }

        public ICollection<BookingDetails>? BookingDetails { get; set; }

        public ICollection<PrescriptionDetails>? PrescriptionDetails { get; set; }  // Add this line

    }
}
