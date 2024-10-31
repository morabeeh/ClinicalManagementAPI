using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ClinicalManagementAPI.Models.Users
{
    public class UserRole
    {
        [Key]
        public int UserRoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string UserRoleName { get; set; }

        [Required]
        public int UserRoleNameId { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public UserDetails User { get; set; }
    }
}
