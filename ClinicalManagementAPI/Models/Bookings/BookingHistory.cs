using ClinicalManagementAPI.Models.Doctors;
using ClinicalManagementAPI.Models.Patients;
using ClinicalManagementAPI.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalManagementAPI.Models.Bookings
{
    public class BookingHistory
    {
        [Key]
        public int BookingHistoryId {  get; set; }

        public DateTime? BookedDate { get; set; }
        public int? BookingId { get; set; }
        [ForeignKey("BookingId")]
        public BookingDetails ?BookingDetails { get; set; }

        public int ?PatientId { get; set; }
        [ForeignKey("PatientId")]
        public PatientDetails ?PatientDetails { get; set; }

        public int ?DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public DoctorDetails ?DoctorDetails { get; set; }
    }
}
