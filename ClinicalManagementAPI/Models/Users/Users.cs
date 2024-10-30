using System.ComponentModel.DataAnnotations;

namespace ClinicalManagementAPI.Models.Users
{
    public class Users
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
        public string Password { get; set; }

        [Required]
        [StringLength(20)]
        public string Dob { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Phone { get; set; }

        public string ? Address { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}
