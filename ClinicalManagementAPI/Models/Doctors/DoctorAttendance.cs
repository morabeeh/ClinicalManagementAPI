using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ClinicalManagementAPI.Models.Doctors
{
    public class DoctorAttendance
    {
        [Key]
        public int DoctorAttendanceId { get; set; }

        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public DoctorDetails Doctor { get; set; }

        public DateTime? TodaysDate { get; set; }
        public bool? IsPresentToday { get; set; }
    }
}
