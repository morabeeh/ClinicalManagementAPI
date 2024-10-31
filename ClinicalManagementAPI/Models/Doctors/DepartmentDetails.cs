using System.ComponentModel.DataAnnotations;

namespace ClinicalManagementAPI.Models.Doctors
{
    public class DepartmentDetails
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; }

        [StringLength(500)]
        public string DepartmentDescription { get; set; }

        public ICollection<DoctorDetails> Doctors { get; set; }
    }
}
