using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ClinicalManagementAPI.Models.Users;
using ClinicalManagementAPI.Models.Patients;
using ClinicalManagementAPI.Models.Bookings;

namespace ClinicalManagementAPI.Models.Doctors
{
    public class DoctorDetails
    {
        [Key]
        public int DoctorId { get; set; }

        public Guid DoctorGuid { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string DoctorName { get; set; }

        [Required]
        [StringLength(100)]
        public string DoctorEducation { get; set; }

        [StringLength(100)]
        public string ? Specialization { get; set; }
        public double? TotalYearExperience { get; set; }


        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public UserDetails User { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
        public DepartmentDetails Department { get; set; }

        public ICollection<DoctorAttendance> DoctorAttendances { get; set; }
        public ICollection<DoctorAvailability> DoctorAvaialabilities { get; set; }

        public ICollection<BookingDetails>? BookingDetails { get; set; }
    }
}
