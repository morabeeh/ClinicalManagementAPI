using ClinicalManagementAPI.Data;
using ClinicalManagementAPI.DataModels.RequestModels;
using ClinicalManagementAPI.Encryption.JWT;
using ClinicalManagementAPI.Models.Users;
using ClinicalManagementAPI.Utility.Mail;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using LoginRequest = ClinicalManagementAPI.DataModels.RequestModels.LoginRequest;
using RegisterRequest = ClinicalManagementAPI.DataModels.RequestModels.RegisterRequest;

namespace ClinicalManagementAPI.Services.AuthenticatoinServices
{
    public interface IAuthenticationServices
    {
        Task<IActionResult> UserRegistration(RegisterRequest regRequest);
        Task<IActionResult> UserLogin(LoginRequest loginRequest);
    }

    public class AuthenticationService : IAuthenticationServices
    {
        private readonly ClinicContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailHelper _mailHelper;

        public AuthenticationService(IOptions<JwtSettings> jwtSettings, ClinicContext context, IMailTemplate mailTemplate, IMailHelper mailHelper)
        {
            _jwtSettings = jwtSettings.Value;
            _context = context;
            _mailTemplate = mailTemplate;
            _mailHelper = mailHelper;
        }


        public async Task<IActionResult> UserRegistration(RegisterRequest regRequest)
        {
            var existingUser = await _context.Users
                .AnyAsync(u => u.EmailAddress == regRequest.EmailAddress && u.CitizenId == regRequest.CitizenId);

            if (existingUser)
            {
                return new ObjectResult(new { error = "User with this Email Address or Citizen ID already exists." })
                {
                    StatusCode = 409 
                };
            }

            var user = new Users
            {
                Name = regRequest.Name,
                CitizenId = regRequest.CitizenId,
                EmailAddress = regRequest.EmailAddress,
                Password = regRequest.Password,
                Dob = regRequest.Dob,
                Gender = regRequest.Gender,
                Address = regRequest.Address,
                Phone = regRequest.Phone
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var userRole = new UserRole
            {
                UserId = user.Id,
                UserRoleName = "Public User"
            };

            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();

            string body = await _mailTemplate.GetWelcomeUserTemplate(user);
            var recipients = new List<string> { user.EmailAddress };

            var validRecipients = recipients
                .Where(recipient => Regex.IsMatch(recipient, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
                .ToArray();

            if (validRecipients.Any())
            {
                _mailHelper.SendEmailInBackground(validRecipients, "Welcome to Al-Huda", body);
            }

            var token = await GenerateJwtToken(user, userRole);

            return new OkObjectResult(new
            {
                statusCode = 200,
                message = "User successfully registered",
                userName = user.Name,
                userRole = userRole.UserRoleName,
                token = token
            });
        }

        public async Task<IActionResult> UserLogin(LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.CitizenId) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return new BadRequestObjectResult(new { message = "Invalid login request." });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.CitizenId == loginRequest.CitizenId && u.Password == loginRequest.Password);

            if (user == null)
            {
                return new UnauthorizedObjectResult(new { statusCode = 401, message = "Invalid CitizenId or password" });
            }

            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == user.Id);

            if (userRole == null)
            {
                return new NotFoundObjectResult(new { statusCode = 404, message = "User role not found" });
            }

            var token = await GenerateJwtToken(user, userRole);

            return new OkObjectResult(new
            {
                statusCode = 200,
                message = "User successfully logged in",
                userName = user.Name,
                userRole = userRole.UserRoleName,
                token = token
            });
        }

        public async Task<string> GenerateJwtToken(Users user, UserRole userRole)
        {
            var key = Encoding.ASCII.GetBytes((_jwtSettings.Key));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, userRole.UserRoleName)
            }),
                Expires = DateTime.UtcNow.AddMinutes(((_jwtSettings.TokenValidityInMinutes))),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = ((_jwtSettings.Issuer)),
                Audience = ((_jwtSettings.Audience))
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
