using ClinicalManagementAPI.Models.Users;

namespace ClinicalManagementAPI.Utility.Mail
{
    public interface IMailTemplate
    {
        Task<string> GetWelcomeUserTemplate(UserDetails user);

        Task<string> GetPatientReport(string user, string doctorName);
    }
    public class MailTemplate:IMailTemplate
    {

        public async Task<string> GetWelcomeUserTemplate(UserDetails user)
        {
            string body = $@"<p style='font-family: Calibri; font-size: 12px; color: #249ee4;'>
                        Dear {user.Name},<br/><br/>
                       Welcome to Al-Huda Poly Clinic . 
                    </p>
                    <p style='font-family: Calibri; font-size: 12px; color: #249ee4; margin-left: 20px;'>
                        <b style='color: #0f3464;'> Book Your Appoinments today!</b><br/>
                        <br/>
                         
                    </p>

                    <p style='font-family: Calibri; font-size: 12px; color: #249ee4;'>Thanks and Regards</p>";

            return body;
        }

        public async Task<string> GetPatientReport(string user,string doctorName)
        {
            string body = $@"<p style='font-family: Calibri; font-size: 12px; color: #249ee4;'>
                        Dear {user},<br/><br/>
                       Here's the Prescription and Medical report of yours with your consultation with Dr. {doctorName} <b>''</b> . 
                    </p>
                    <p style='font-family: Calibri; font-size: 12px; color: #249ee4; margin-left: 20px;'>
                        <b style='color: #0f3464;'>Project Details:</b><br/>
                         Thank you for booking with Al-Huda Clinic, We hope and wish all good for your health, and please consult with us on future needs<br/>
                         
                    </p>

                    <p style='font-family: Calibri; font-size: 12px; color: #249ee4;'>Thanks and Regards,<br/> {user}</p>";

            return body;
        }
    }
}
