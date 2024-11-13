namespace ClinicalManagementAPI.DataModels.ResponseModels
{
    public class PatientBookingsResponseDto
    {
        public PatientDto Patient { get; set; }
        public List<BookingDto> Bookings { get; set; }

        public class PatientDto
        {
            public int PatientId { get; set; }
            public string PatientGuid { get; set; }
            public string PatientName { get; set; }
            public string PatientDescription { get; set; }
            public string PatientHealthCondition { get; set; }
        }

        public class BookingDto
        {
            public int BookingId { get; set; }
            public int? BookingToken { get; set; }
            public string BookingStatus { get; set; }
            public DateTime? BookingDateTime { get; set; }
            public DoctorDto Doctor { get; set; }
            public List<BookingHistoryDto> BookingHistories { get; set; }
        }

        public class DoctorDto
        {
            public int DoctorId { get; set; }
            public string DoctorGuid { get; set; }
            public string DoctorName { get; set; }
            public string DoctorEducation { get; set; }
            public string Specialization { get; set; }
            public double? TotalYearExperience { get; set; }
            public DepartmentDto Department { get; set; }
        }

        public class DepartmentDto
        {
            public int DepartmentId { get; set; }
            public string DepartmentName { get; set; }
            public string DepartmentDescription { get; set; }
        }

        public class BookingHistoryDto
        {
            public int BookingHistoryId { get; set; }
            public DateTime? BookedDate { get; set; }
        }
    }

}
