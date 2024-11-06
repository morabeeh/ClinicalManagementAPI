namespace ClinicalManagementAPI.DataModels.RequestModels
{
    public class PatientBookingRequestDto
    {
        public int PatientId { get; set; }
        public int BookingId { get; set; }
        public int DoctorId { get; set; }
    }

}
