using System.ComponentModel.DataAnnotations;

namespace ClinicalManagementAPI.DataModels.ResponseModels
{
    public class DoctorDto
    {
        public int UserId { get; set; }
        public int DoctorId { get; set; }
        public Guid DoctorGuid { get; set; }
        public string DoctorName { get; set; }
        public string DoctorEducation { get; set; }
        public string? Specialization { get; set; }
        public double? TotalYearExperience { get; set; }
        public string CitizenId { get; set; } // From UserDetails
        public DepartmentDto Department { get; set; }
        public List<AvailabilityDto> Availabilities { get; set; }
        public List<AttendanceDto> Attendances { get; set; }


    }

    public class DepartmentDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string? DepartmentDescription { get; set; }
    }

    public class AvailabilityDto
    {
        public string DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
    public class AttendanceDto
    {
        public DateTime? TodaysDate { get; set; }
        public bool? IsPresentToday { get; set; }
    }

}
