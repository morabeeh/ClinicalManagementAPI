using System.ComponentModel.DataAnnotations;

namespace ClinicalManagementAPI.DataModels.RequestModels
{
    public class AssignDoctorRequest
    {
        [Required]
        public int UserId { get; set; }

        //Doctor Details

        [Required]
        [StringLength(100)]
        public string DoctorEducation { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; }

        public double? TotalYearExperience { get; set; }

        //Department Details
        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; }

        [StringLength(500)]
        public string DepartmentDescription { get; set; }
        public int? DepartmentId { get; set; }

    }

    //Availability
    public class AddDoctorAvailabilityRequest
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        [StringLength(50)]
        public string DayOfWeek { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public string StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public string EndTime { get; set; }
    }

    //Attendance
    public class AddDoctorAttendanceRequest
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime TodaysDate { get; set; }

        [Required]
        public bool IsPresentToday { get; set; }
    }
}
