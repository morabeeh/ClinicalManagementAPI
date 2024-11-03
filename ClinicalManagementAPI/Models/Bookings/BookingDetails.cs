using ClinicalManagementAPI.Models.Doctors;
using ClinicalManagementAPI.Models.Patients;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalManagementAPI.Models.Bookings
{
    public class BookingDetails
    {
        [Key]
        public int BookingId { get; set; }

        public int ? BookingToken { get; set; }

        public string? BookingStatus { get; set; }

        public DateTime? BookingDateTime { get; set; }

        public bool IsBookingCancelled { get; set; } = false;
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public PatientDetails? PatientDetails { get; set; }

        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public DoctorDetails? DoctorDetails { get; set; }
        public ICollection<BookingHistory> BookingHistory { get; set; }
    }
}
