namespace ClinicalManagementAPI.DataModels.ResponseModels
{
    public class PatientPrescriptionsResponseDto
    {
        public PatientDto Patient { get; set; }
        public DoctorDto Doctor { get; set; }
        public List<PrescriptionDto> Prescriptions { get; set; }

        public class PatientDto
        {
            public int PatientId { get; set; }
            public string PatientGuid { get; set; }
            public string PatientName { get; set; }
            public string PatientDescription { get; set; }
            public string PatientHealthCondition { get; set; }

            // New property to indicate if the user has already rated
            public bool UserAlreadyRate { get; set; }
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

        public class PrescriptionDto
        {
            public int PrescriptionId { get; set; }
            public string PrescriptionName { get; set; }
            public string PrescriptionDescription { get; set; }
            public string DeseaseName { get; set; }
            public string DeseaseType { get; set; }
            public string DeseaseDescription { get; set; }
            public double? MorningDosage { get; set; }
            public double? NoonDosage { get; set; }
            public double? NightDosage { get; set; }
            public double? OtherDosage { get; set; }
            public string MorningDosageTime { get; set; }
            public string NoonDosageTime { get; set; }
            public string NightDosageTime { get; set; }
            public string OtherDosageTime { get; set; }
            public string DoctorAdvices { get; set; }
            public DateTime? PrescribedDate { get; set; }

            public int? BookingId { get; set; }
            public BookingDetailsDto BookingDetails { get; set; } // Added BookingDetails property
        }

        public class BookingDetailsDto
        {
            public int BookingId { get; set; }
            public int? BookingToken { get; set; }
            public string BookingStatus { get; set; }
            public DateTime? BookingDateTime { get; set; }
            public bool IsBookingCancelled { get; set; }
        }
    }

}


