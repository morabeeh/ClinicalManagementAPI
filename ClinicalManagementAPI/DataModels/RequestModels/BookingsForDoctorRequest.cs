namespace ClinicalManagementAPI.DataModels.RequestModels
{
    public class BookingsForDoctorRequestDto
    {
        public int DoctorId { get; set; }
        public DateTime BookingDateTime { get; set; }
    }
}
