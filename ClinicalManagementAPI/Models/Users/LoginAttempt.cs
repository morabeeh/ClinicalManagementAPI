namespace ClinicalManagementAPI.Models.Users
{
    public class LoginAttempt
    {
        public int Id { get; set; }

        public Guid LoginGuid { get; set; } = Guid.NewGuid();
        public string IpAddress { get; set; } = null!;       
        public int AttemptCount { get; set; }                
        public DateTime WindowStart { get; set; }             
        public DateTime? LockedUntil { get; set; }           
    }
}
