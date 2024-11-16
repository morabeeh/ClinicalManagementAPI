using ClinicalManagementAPI.Models.Bookings;
using ClinicalManagementAPI.Models.Doctors;
using ClinicalManagementAPI.Models.Patients;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalManagementAPI.Models.Users
{
    public class UserRatings
    {

        [Key]
        public int UserRatingsId { get; set; }

        public string? PatientWhoRated { get; set; }


        //doctors

        public string? RatedDoctor { get; set; }

        public double? DoctorRatingsValue { get; set; }

        public string? PatientFeedbackForDoctor { get; set; }


        //clinic
        public string? PatientFeedbackForClinic { get; set; }

        public double? ClinicRatingValue { get; set; }

        public int? PatientId { get; set; }
        [ForeignKey("PatientId")]
        public PatientDetails? PatientDetails { get; set; }

        public int? DoctorId { get; set; }
        [ForeignKey("PatientId")]
        public DoctorDetails? DoctorDetails { get; set; }

        public int? BookingId { get; set; }
        [ForeignKey("BookingId")]
        public BookingDetails? BookingDetails { get; set; }


    }
}
