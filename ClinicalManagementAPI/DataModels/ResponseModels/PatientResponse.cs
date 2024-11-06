namespace ClinicalManagementAPI.DataModels.ResponseModels
{
    // Response DTO
    public class PatientBookingResponseDto
    {
        public PatientResponseDetailsDto PatientDetails { get; set; }
        public List<PatientResponseHistoryDto> PatientHistories { get; set; }
        public List<PrescriptionResponseDto> PrescriptionDetails { get; set; } // Add prescription details
        public PatientBookingDetailsDto BookingDetails { get; set; }
        public List<PatientBookingHistoryDto> BookingHistories { get; set; }
    }

    // Prescription Details DTO
    public class PrescriptionResponseDto
    {
        public int? PrescriptionId { get; set; }

        public string? PrescriptionName { get; set; }
        public string? PrescriptionDescription { get; set; }

        public string? DeseaseName { get; set; }
        public string? DeseaseDescription { get; set; }
        public string? DeseaseType { get; set; }

        public double? MorningDosage { get; set; }
        public string ? MorningDosageTime { get; set; }

        public double? NoonDosage { get; set; }
        public string? NoonDosageTime { get; set; }

        public double? NightDosage { get; set; }
        public string? NightDosageTime { get; set; }

        public double? OtherDosage { get; set; }
        public string? OtherDosageTime { get; set; }

        public string? DoctorAdvices { get; set; }

        public DateTime? ConsultedDate { get; set; }

        
    }

    // Patient Details DTO
    public class PatientResponseDetailsDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string PatientDescription { get; set; }
        public string PatientHealthCondition { get; set; }
        public string Gender { get; set; }
        public string Dob { get; set; }
        public string Address { get; set; }
    }

    // Patient History DTO
    public class PatientResponseHistoryDto
    {
        public int HistoryId { get; set; }
        public DateTime? ConsultedDate { get; set; }
        public string ConsultedDoctor { get; set; }
    }

    // Booking Details DTO
    public class PatientBookingDetailsDto
    {
        public int BookingId { get; set; }
        public int? BookingToken { get; set; }
        public string BookingStatus { get; set; }
        public DateTime? BookingDateTime { get; set; }
        public bool IsBookingCancelled { get; set; }
    }

    // Booking History DTO
    public class PatientBookingHistoryDto
    {
        public int BookingHistoryId { get; set; }
        public DateTime? BookedDate { get; set; }
    }


}
