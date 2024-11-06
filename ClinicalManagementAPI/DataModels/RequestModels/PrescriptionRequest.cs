namespace ClinicalManagementAPI.DataModels.RequestModels
{
    public class PrescriptionRequestDto
    {
        public int PatientId { get; set; }
        public int BookingId { get; set; }
        public int DoctorId { get; set; }

        public string? PrescriptionName { get; set; }
        public string ? PrescriptionDescription { get; set; }
        public string? PrescriptionDetails { get; set; }
        public string ?DeseaseName { get; set; }
        public string ?DeseaseDescription { get; set; }
        public double? MorningDosage { get; set; }
        public double? NoonDosage { get; set; }
        public double? NightDosage { get; set; }
        public double? OtherDosage { get; set; }
        public string ? MorningDosageTime { get; set; }
        public string ?NoonDosageTime { get; set; }
        public string ?NightDosageTime { get; set; }
        public string ?OtherDosageTime { get; set; }
        public string ?DoctorAdvices { get; set; }
        
    }

}
