using ClinicalManagementAPI.Data;
using ClinicalManagementAPI.DataModels.RequestModels;
using ClinicalManagementAPI.Encryption.JWT;
using ClinicalManagementAPI.Models.Users;
using ClinicalManagementAPI.Services.AuthenticatoinServices;
using ClinicalManagementAPI.Utility.Mail;
using Microsoft.AspNetCore.Authentication;
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

namespace ClinicalManagementAPI.Controllers
{
    

    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationServices _authenticationService;

        public AuthenticationController(IAuthenticationServices authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest regRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authenticationService.UserRegistration(regRequest);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(404, new
                {
                    statusCode = 404,
                    message = "An error occurred during registration. Please try again later.",
                    exception= ex
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authenticationService.UserLogin(loginRequest);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(404, new
                {
                    statusCode = 404,
                    message = "An error occurred during login. Please try again later.",
                    exception = ex
                });
            }
        }



        //temprary api for hasing the existing password



    }

}
