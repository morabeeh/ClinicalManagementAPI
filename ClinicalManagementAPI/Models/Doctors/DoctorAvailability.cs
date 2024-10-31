using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ClinicalManagementAPI.Models.Doctors
{
    public class DoctorAvailability
    {
        [Key]
        public int AvailabilityId { get; set; }

        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public DoctorDetails Doctor { get; set; }

        [Required]
        [StringLength(50)]
        public string DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }
    }
}
