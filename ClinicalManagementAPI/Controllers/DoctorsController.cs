using ClinicalManagementAPI.Data;
using ClinicalManagementAPI.DataModels.RequestModels;
using ClinicalManagementAPI.DataModels.ResponseModels;
using ClinicalManagementAPI.Migrations;
using ClinicalManagementAPI.Models.Doctors;
using ClinicalManagementAPI.Models.Users;
using ClinicalManagementAPI.Services.DoctorLogicService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly ClinicContext _context;
        private readonly IDoctorLogicService _doctorLogicService;

        public DoctorsController(ClinicContext context,IDoctorLogicService doctorLogicService)
        {
            _context = context;
            _doctorLogicService = doctorLogicService;   
        }

        [HttpPost("assignDoctors")]
        public async Task<IActionResult> AssignDoctor([FromBody] AssignDoctorRequest request)
        {
            // Validate the request
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var department = await _doctorLogicService.AddOrUpdateDepartment(request);

            var doctor = await _doctorLogicService.AddOrUpdateDoctor(request, user, department);

            await _doctorLogicService.UpdateUserRole(request);

            return Ok(new { message = "Doctor assigned successfully", doctorId = doctor.DoctorId,doctorName=doctor.DoctorName });
        }

        

        



        // POST: api/doctors/addAvailability
        [HttpPost("addAvailability")]
        public async Task<IActionResult> AddDoctorAvailability([FromBody] AddDoctorAvailabilityRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if doctor exists
            var doctor = await _context.Doctors.FindAsync(request.DoctorId);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });

            if (!TimeSpan.TryParse(request.StartTime, out TimeSpan startTime))
                return BadRequest("Invalid start time format. Use 'HH:mm'.");

            if (!TimeSpan.TryParse(request.EndTime, out TimeSpan endTime))
                return BadRequest("Invalid end time format. Use 'HH:mm'.");

            var availability = new DoctorAvailability
            {
                DoctorId = request.DoctorId,
                DayOfWeek = request.DayOfWeek,
                StartTime = startTime,
                EndTime = endTime
            };

            _context.DoctorAvailabilities.Add(availability);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor availability added successfully" });
        }


        // POST: api/doctors/addAttendance
        [HttpPost("addAttendance")]
        public async Task<IActionResult> AddDoctorAttendance([FromBody] AddDoctorAttendanceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if doctor exists
            var doctor = await _context.Doctors.FindAsync(request.DoctorId);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });

            // Create and save DoctorAttendance
            var attendance = new DoctorAttendance
            {
                DoctorId = request.DoctorId,
                TodaysDate = DateTime.Now,
                IsPresentToday = request.IsPresentToday
            };

            _context.DoctorAttendances.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor attendance recorded successfully" });
        }



        [HttpGet("getUsersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var users = await _context.Users
                        .Include(u => u.UserRoles)
                        .Where(u => u.UserRoles.Any()) // Only include users with roles
                        .ToListAsync();

            // Map to DTOs
            var userDtos = users.Select(u => new UserWithRolesDto
            {
                UserId = u.Id,
                Name = u.Name,
                CitizenId = u.CitizenId,
                EmailAddress = u.EmailAddress,
                Dob = u.Dob,
                Gender = u.Gender,
                Phone = u.Phone,
                Address = u.Address,
                UserRoles = u.UserRoles.Select(ur => new UserRoleDto
                {
                    UserRoleId = ur.UserRoleId,
                    UserRoleName = ur.UserRoleName,
                    UserRoleNameId = ur.UserRoleNameId
                }).ToList()
            }).ToList();

            return Ok(userDtos);
        }

        [HttpGet("getAllDoctorDetails")]
        public async Task<IActionResult> GetAllDoctorDetails()
        {
            try
            {
                var doctors = await _context.Doctors
                    .Include(d => d.Department)
                    .Include(d => d.User)
                    .Include(d => d.DoctorAvaialabilities)
                    .Select(d => new DoctorDto
                    {
                        UserId = d.User.Id,
                        DoctorId = d.DoctorId,
                        DoctorGuid = d.DoctorGuid,
                        DoctorName = d.DoctorName,
                        DoctorEducation = d.DoctorEducation,
                        Specialization = d.Specialization,
                        TotalYearExperience = d.TotalYearExperience,
                        CitizenId = d.User.CitizenId,
                        Department = new DepartmentDto
                        {
                            DepartmentId = d.Department.DepartmentId,
                            DepartmentName = d.Department.DepartmentName,
                            DepartmentDescription = d.Department.DepartmentDescription
                        },
                        Availabilities = d.DoctorAvaialabilities.Select(a => new AvailabilityDto
                        {
                            DayOfWeek = a.DayOfWeek,
                            StartTime = a.StartTime,
                            EndTime = a.EndTime
                        }).ToList()
                    })
                    .ToListAsync();

                if (doctors == null || !doctors.Any())
                {
                    return NotFound(new { Message = "No Records Found" });
                }

                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the doctor details.", Details = ex.Message });
            }
        }

        [HttpGet("getDoctorDetails")]
        public async Task<IActionResult> GetDoctorDetails(int userId)
        {
            try
            {
                var doctors = await _context.Doctors
                    .Include(d => d.Department)
                    .Include(d => d.User)
                    .Include(d => d.DoctorAvaialabilities)
                    .Include(d => d.DoctorAttendances)
                    .Where(d => d.UserId == userId)
                    .Select(d => new DoctorDto
                    {
                        UserId = d.User.Id,
                        DoctorId = d.DoctorId,
                        DoctorGuid = d.DoctorGuid,
                        DoctorName = d.DoctorName,
                        DoctorEducation = d.DoctorEducation,
                        Specialization = d.Specialization,
                        TotalYearExperience = d.TotalYearExperience,
                        CitizenId = d.User.CitizenId,
                        Department = new DepartmentDto
                        {
                            DepartmentId = d.Department.DepartmentId,
                            DepartmentName = d.Department.DepartmentName,
                            DepartmentDescription = d.Department.DepartmentDescription
                        },
                        Availabilities = d.DoctorAvaialabilities.Select(a => new AvailabilityDto
                        {
                            DayOfWeek = a.DayOfWeek,
                            StartTime = a.StartTime,
                            EndTime = a.EndTime
                        }).ToList(),
                        Attendances = d.DoctorAttendances.Select(a => new AttendanceDto
                        {
                            TodaysDate = a.TodaysDate,
                            IsPresentToday = a.IsPresentToday
                        }).ToList()
                    })
                    .ToListAsync();

                if (doctors == null || !doctors.Any())
                {
                    return NotFound(new { Message = "No Records Found" });
                }

                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the doctor details.", Details = ex.Message });
            }
        }

    }
}
