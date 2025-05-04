using System.ComponentModel.DataAnnotations;
using ClinicalManagementAPI.Models.Doctors;
using ClinicalManagementAPI.Models.Patients;

namespace ClinicalManagementAPI.Models.Users
{
    public class UserDetails
    {
        [Key]
        public int Id { get; set; }

        public Guid UserGuid { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public string CitizenId { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; }


        [Required]
        [StringLength(20)]
        public string Dob { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Phone { get; set; }

        public string ? Address { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }

        public ICollection <DoctorDetails> Doctors { get; set; }

        public ICollection <PatientDetails> Patients { get; set; }
    }
}
