using ClinicalManagementAPI.Models.Users;

namespace ClinicalManagementAPI.Utility.Mail
{
    public interface IMailTemplate
    {
        Task<string> GetWelcomeUserTemplate(Users user);
    }
    public class MailTemplate:IMailTemplate
    {

        public async Task<string> GetWelcomeUserTemplate(Users user)
        {
            string body = $@"<p style='font-family: Calibri; font-size: 12px; color: #249ee4;'>
                        Dear {user.Name},<br/><br/>
                       Welcome to Al-Huda Poly Clinic <b>''</b> . 
                    </p>
                    <p style='font-family: Calibri; font-size: 12px; color: #249ee4; margin-left: 20px;'>
                        <b style='color: #0f3464;'>Project Details:</b><br/>
                         Book Your Appoinments today!<br/>
                         
                    </p>

                    <p style='font-family: Calibri; font-size: 12px; color: #249ee4;'>Thanks and Regards,<br/> {user}</p>";

            return body;
        }
    }
}
