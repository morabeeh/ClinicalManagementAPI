using System.ComponentModel.DataAnnotations;

namespace ClinicalManagementAPI.DataModels.RequestModels
{
    public class RegisterRequest
    {

        [Required]
        [StringLength(100)]
        public string  Name { get; set; }

        [Required]
        [StringLength(20)]
        public string  CitizenId { get; set; }

        [Required]
        [EmailAddress]
        public string  EmailAddress { get; set; }

        [Required]
        [StringLength(200)]
        public string Password{ get; set; }

        [Required]
        [StringLength(30)]
        public string Dob { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Phone { get; set; }

        public string Address { get; set; }
    }
}
