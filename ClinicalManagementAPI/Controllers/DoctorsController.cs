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

            // Check if availability overlaps with existing ones
            //var overlappingAvailability = await _context.DoctorAvailabilities
            //    .AnyAsync(d => d.DoctorId == request.DoctorId &&
            //                   d.DayOfWeek == request.DayOfWeek &&
            //                   ((startTime >= d.StartTime && startTime < d.EndTime) ||
            //                    (endTime > d.StartTime && endTime <= d.EndTime) ||
            //                    (startTime <= d.StartTime && endTime >= d.EndTime)));

            //if (overlappingAvailability)
            //    return BadRequest(new { message = "The specified time range overlaps with existing availability." });


            // Fetch existing availability if present
            var existingAvailability = await _context.DoctorAvailabilities
                .FirstOrDefaultAsync(d => d.DoctorId == request.DoctorId && d.DayOfWeek == request.DayOfWeek);

            if (existingAvailability == null)
            {
                // Add new availability
                var availability = new DoctorAvailability
                {
                    DoctorId = request.DoctorId,
                    DayOfWeek = request.DayOfWeek,
                    StartTime = startTime,
                    EndTime = endTime
                };

                _context.DoctorAvailabilities.Add(availability);
            }
            else
            {
                // Update existing availability
                existingAvailability.DayOfWeek = request.DayOfWeek;
                existingAvailability.StartTime = startTime;
                existingAvailability.EndTime = endTime;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Doctor availability added successfully" });
        }

        [HttpGet("availableDoctors")]
        public async Task<IActionResult> GetAvailableDoctors([FromQuery] DateTime? date = null)
        {
            var selectedDate = date?.Date ?? DateTime.Today.Date;  // Only take the date part
            var dayOfWeek = selectedDate.DayOfWeek.ToString();
            var isToday = selectedDate == DateTime.Today.Date;

            // Fetch doctors based on selectedDate conditionally
            var doctors = await _context.Doctors
                .Include(d => d.Department)
                .Include(d => d.User)
                .Include(d => d.DoctorAvaialabilities)
                .Include(d => d.DoctorAttendances)
                .Where(d => d.DoctorAvaialabilities.Any(a => a.DayOfWeek == dayOfWeek) &&
                            // Conditionally check DoctorAttendances if date is today's date
                            (!isToday || d.DoctorAttendances.Any(att =>
                                att.TodaysDate.HasValue &&
                                att.TodaysDate.Value.Date == selectedDate &&
                                att.IsPresentToday == true))
                        )
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
                    Gender=d.User.Gender,
                    Department = new DepartmentDto
                    {
                        DepartmentId = d.Department.DepartmentId,
                        DepartmentName = d.Department.DepartmentName,
                        DepartmentDescription = d.Department.DepartmentDescription
                    },
                    Availabilities = d.DoctorAvaialabilities
                        .Where(a => a.DayOfWeek == dayOfWeek)
                        .Select(a => new AvailabilityDto
                        {
                            DayOfWeek = a.DayOfWeek,
                            StartTime = a.StartTime,
                            EndTime = a.EndTime
                        }).ToList(),
                    AverageRating = (int?)(_context.UserRatings
                                    .Where(r => r.DoctorId == d.DoctorId)
                                    .Select(r => (double?)r.DoctorRatingsValue)
                                    .Average()
                                    .HasValue
                                        ? (double?)Math.Ceiling(_context.UserRatings
                                              .Where(r => r.DoctorId == d.DoctorId)
                                              .Select(r => (double?)r.DoctorRatingsValue)
                                              .Average() ?? 0)
                                        : null),
                                        TotalRatings = _context.UserRatings
                                    .Count(r => r.DoctorId == d.DoctorId)// Default to 0 if no ratings
                })
                .ToListAsync();

            return Ok(doctors);
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
                        .Where(u => u.UserRoles.Any(role => role.UserRoleNameId != 10)) // Only include users with roles that are not 10
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
                    .Include(d => d.DoctorAttendances)
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

        [HttpGet("getSpecificDoctorDetails")]
        public async Task<IActionResult> GetSpecificDoctorDetails(int doctorId)
        {
            try
            {
                var doctors = await _context.Doctors
                    .Include(d => d.Department)
                    .Include(d => d.User)
                    .Include(d => d.DoctorAvaialabilities)
                    .Where(d => d.DoctorId == doctorId)
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
                        Gender=d.User.Gender,
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


        [HttpGet("completed-bookings")]
        public async Task<IActionResult> GetCompletedPatientDetails(int doctorId)
        {
            try
            {
                var patientDetailsList = await _context.BookingDetails
                    .Where(b => b.DoctorId == doctorId && b.BookingStatus == "Booking Completed")
                    .Include(b => b.PatientDetails)
                        .ThenInclude(p => p.PatientHistory
                            .Where(h => h.ConsultedDoctor == _context.Doctors
                                            .Where(d => d.DoctorId == doctorId)
                                            .Select(d => d.DoctorName)
                                            .FirstOrDefault()))
                    .Include(b => b.PatientDetails)
                        .ThenInclude(p => p.BookingDetails)
                    .Include(b => b.BookingHistory)
                    .Include(b => b.PatientDetails)
                        .ThenInclude(p => p.User)
                    .Select(b => b.PatientDetails) // Select only unique PatientDetails
                    .Distinct()
                    .ToListAsync();

                if (patientDetailsList == null || !patientDetailsList.Any())
                {
                    return NotFound("No completed bookings found for this doctor.");
                }

                var result = patientDetailsList.Select(p => new
                {
                    PatientId = p.PatientId,
                    PatientGuid = p.PatientGuid,
                    PatientName = p.PatientName,
                    PatientDescription = p.PatientDescription,
                    PatientHealthCondition = p.PatientHealthCondition,
                    UserDetails = p.User == null ? null : new
                    {
                        UserId = p.User.Id,
                        UserGuid = p.User.UserGuid,
                        Name = p.User.Name,
                        EmailAddress = p.User.EmailAddress,
                        Dob = p.User.Dob,
                        Gender = p.User.Gender,
                        Phone = p.User.Phone,
                        Address = p.User.Address
                    },
                    BookingDetails = p.BookingDetails?
                        .Where(b => b.BookingStatus == "Booking Completed" && b.DoctorId==doctorId)
                        .Select(b => new
                        {
                            BookingId = b.BookingId,
                            BookingStatus = b.BookingStatus,
                            BookingDateTime = b.BookingDateTime,
                            BookingHistory = b.BookingHistory?
                                .Select(h => new
                                {
                                    BookingHistoryId = h.BookingHistoryId,
                                    BookedDate = h.BookedDate,
                                    BookingId = h.BookingId,
                                    DoctorId = h.DoctorId
                                })
                                .ToList()
                        })
                        .ToList(),
                    PatientHistory = p.PatientHistory?
                        .Where(h => h.ConsultedDoctor == _context.Doctors
                                        .Where(d => d.DoctorId == doctorId)
                                        .Select(d => d.DoctorName)
                                        .FirstOrDefault())
                        .Select(h => new
                        {
                            HistoryId = h.HistoryId,
                            ConsultedDoctor = h.ConsultedDoctor,
                            ConsultedDate = h.ConsultedDate,
                            BookingId = h.BookingId,
                            DoctorId = h.DoctorId
                        })
                        .ToList()
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the error here if needed
                return StatusCode(500, "An error occurred while retrieving data.");
            }
        }


    }
}
