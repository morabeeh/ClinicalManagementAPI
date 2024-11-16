namespace ClinicalManagementAPI.DataModels.RequestModels
{
    public class UserRatingsRequestDto
    {
        public int PatientId { get; set; }
        public int BookingId { get; set; }
        public int? DoctorId { get; set; }
        public double? DoctorRatingsValue { get; set; }
        public string? PatientFeedbackForDoctor { get; set; }
        public double? ClinicRatingValue { get; set; }
        public string? PatientFeedbackForClinic { get; set; }
    }

}
