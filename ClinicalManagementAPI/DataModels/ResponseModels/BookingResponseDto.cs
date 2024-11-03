namespace ClinicalManagementAPI.DataModels.ResponseModels
{
    public class BookingResponseDto
    {
        public int BookingToken { get; set; }
        public string BookingStatus { get; set; }
        public DateTime BookingDateTime { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string Department { get; set; }
    }

}
