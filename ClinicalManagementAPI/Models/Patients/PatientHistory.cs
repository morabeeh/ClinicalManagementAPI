using ClinicalManagementAPI.Models.Bookings;
using ClinicalManagementAPI.Models.Doctors;
using ClinicalManagementAPI.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalManagementAPI.Models.Patients
{
    public class PatientHistory
    {
        [Key]
        public int HistoryId { get; set; }

        public Guid HistoryGuid { get; set; } = Guid.NewGuid();

        //User Details
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public UserDetails ?User { get; set; }

        //Patient Details
        public int? PatientId { get; set; }
        [ForeignKey("PatientId")]
        public PatientDetails ?Patient { get; set; }

        //Doctor Details
        public string ?ConsultedDoctor {  get; set; }
        public DateTime ?ConsultedDate { get; set; }
        public int? DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public DoctorDetails ?Doctor { get; set; }

        public int? BookingHistoryId { get; set; }
        [ForeignKey("BookingHistoryId")]
        public BookingHistory? BookingHistory { get; set; }

    }
}
