namespace ClinicalManagementAPI.DataModels.RequestModels
{
    public class BookAppointmentRequestDto
    {
        public int UserId { get; set; }
        public int DoctorId { get; set; }
        public DateTime BookingDateTime { get; set; }
    }

}
