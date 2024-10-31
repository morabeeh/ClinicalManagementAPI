namespace ClinicalManagementAPI.DataModels.ResponseModels
{
    public class UserWithRolesDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string CitizenId { get; set; }
        public string EmailAddress { get; set; }
        public string Dob { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string? Address { get; set; }
        public List<UserRoleDto> UserRoles { get; set; }
    }

    public class UserRoleDto
    {
        public int UserRoleId { get; set; }
        public string UserRoleName { get; set; }
        public int UserRoleNameId { get; set; }
    }
}
