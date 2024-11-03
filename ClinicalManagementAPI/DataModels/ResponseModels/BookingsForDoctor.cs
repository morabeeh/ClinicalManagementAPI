﻿namespace ClinicalManagementAPI.DataModels.ResponseModels
{
    public class BookingForDoctorResponseDto
    {
        public PatientDetailsDto PatientDetails { get; set; }
        public BookingDetailsDto BookingDetails { get; set; }
        public List<PatientHistoryDto> PatientHistory { get; set; }
    }

    public class PatientDetailsDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        // Add other necessary properties from PatientDetails
    }

    public class BookingDetailsDto
    {
        public int BookingId { get; set; }
        public int? BookingToken { get; set; }
        public string BookingStatus { get; set; }
        public DateTime? BookingDateTime { get; set; }
        public bool IsBookingCancelled { get; set; }
    }

    public class PatientHistoryDto
    {
        public int HistoryId { get; set; }
        public DateTime ?ConsultedDate { get; set; }
        public string ConsultedDoctor { get; set; }
        // Add other necessary properties from PatientHistory
    }

}