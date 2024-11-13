using ClinicalManagementAPI.Data;
using ClinicalManagementAPI.DataModels.RequestModels;
using ClinicalManagementAPI.DataModels.ResponseModels;
using ClinicalManagementAPI.Models.Bookings;
using ClinicalManagementAPI.Models.Doctors;
using ClinicalManagementAPI.Models.Patients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicalManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly ClinicContext _context;

        public BookingController(ClinicContext context)
        {
            _context = context;
        }

        [HttpGet("getBookingsForDoctor")]
        public async Task<IActionResult> GetBookingsForDoctor(int doctorId, DateTime bookingDateTime)
        {

            var bookingDateTimeUtc = DateTime.SpecifyKind(bookingDateTime, DateTimeKind.Utc).Date;

            var bookings = await _context.BookingDetails
                .Include(b => b.PatientDetails) // Include PatientDetails
                .Include(b => b.PatientDetails.PatientHistory) // Include PatientHistory
                .Where(b => b.DoctorId == doctorId &&( b.BookingDateTime.Value.Date == bookingDateTimeUtc && b.IsBookingCancelled == false && b.BookingStatus=="Booking Confirmed"))
                .ToListAsync();

            if (bookings == null || !bookings.Any())
            {
                return NotFound(new { message = "Doctor not available at this time" });
            }

            var response = bookings.Select(b => new BookingForDoctorResponseDto
            {
                BookingDetails = new BookingDetailsDto
                {
                    BookingId = b.BookingId,
                    BookingToken = b.BookingToken,
                    BookingStatus = b.BookingStatus,
                    BookingDateTime = b.BookingDateTime,
                    IsBookingCancelled = b.IsBookingCancelled
                },
                PatientDetails = new PatientDetailsDto
                {
                    PatientId = b.PatientDetails.PatientId,
                    PatientName = b.PatientDetails.PatientName,
                    // Map other properties as needed
                },
                PatientHistory = b.PatientDetails.PatientHistory.Select(ph => new PatientHistoryDto
                {
                    HistoryId = ph.HistoryId,
                    ConsultedDate = ph?.ConsultedDate,
                    ConsultedDoctor = ph.ConsultedDoctor,
                    // Map other properties as needed
                }).ToList()
            }).ToList();

            return Ok(response);
        }

        [HttpPost("cancelAppointment")]
        public async Task<IActionResult> CancelAppointment([FromBody] CancelAppointmentRequestDto request)
        {
            var bookings = await _context.BookingDetails.FirstOrDefaultAsync(b => b.BookingId == request.BookingId);

            if (bookings != null)
            {
                bookings.BookingStatus = "Booking Cancelled";
                bookings.IsBookingCancelled = true;
            }

            await _context.SaveChangesAsync();
            
            return Ok(new {message="Booking Has Cancelled"});
        }


        [HttpPost("bookAppointment")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequestDto request)
        {
            // Map Patients from users
            var user = await _context.Users
                .FirstOrDefaultAsync(p => p.Id == request.UserId);

            var patient = await _context.PatientDetails
                .FirstOrDefaultAsync(p => p.UserId == request.UserId);

            if (patient == null)
            {
                patient = new PatientDetails
                {
                    PatientName = user?.Name,
                    UserId = request.UserId
                };

                _context.PatientDetails.Add(patient);
                await _context.SaveChangesAsync();
            }

            // Ensure the bookingDateTime is in UTC if needed
            var bookingDateTimeUtc = DateTime.SpecifyKind(request.BookingDateTime, DateTimeKind.Utc);
            var dayOfWeek = bookingDateTimeUtc.DayOfWeek.ToString();

            // Step 1: Check if booking is in the past (for today only)
            if (bookingDateTimeUtc.Date == DateTime.UtcNow.Date && bookingDateTimeUtc < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Booking time cannot be in the past for today." });
            }

            // Step 2: Check Doctor Availability for the given date and time
            var doctorAvailability = await _context.DoctorAvailabilities
                .Where(a => a.DoctorId == request.DoctorId && a.DayOfWeek == dayOfWeek)
                .FirstOrDefaultAsync();

            if (doctorAvailability == null ||
                bookingDateTimeUtc.TimeOfDay < doctorAvailability.StartTime ||
                bookingDateTimeUtc.TimeOfDay > doctorAvailability.EndTime)
            {
                return BadRequest(new { message = "Doctor not available at this time." });
            }

            // Step 3: Check if the user already has two bookings on the same day
            var sameDayBookings = await _context.BookingDetails
                .Where(b => b.PatientId == patient.PatientId &&
                            b.BookingDateTime.HasValue &&
                            b.BookingDateTime.Value.Date == request.BookingDateTime.Date &&
                            !b.IsBookingCancelled) // Only count active bookings
                .CountAsync();

            if (sameDayBookings >= 2)
            {
                return BadRequest(new { message = "You can only book up to two appointments per day." });
            }

            // Step 4: Check for Existing Bookings that are not cancelled
            var existingBookings = await _context.BookingDetails
                .Where(b => b.DoctorId == request.DoctorId &&
                             b.BookingDateTime.Value.Date == request.BookingDateTime.Date &&
                             !b.IsBookingCancelled) // Check for non-cancelled bookings
                .OrderBy(b => b.BookingDateTime)
                .ToListAsync();

            // Each booking is 10 mins; check if time slot is available
            var bookingEnd = request.BookingDateTime.AddMinutes(10);
            foreach (var booking in existingBookings)
            {
                var bookedEnd = booking.BookingDateTime.Value.AddMinutes(10);
                if ((request.BookingDateTime >= booking.BookingDateTime && request.BookingDateTime < bookedEnd) ||
                    (bookingEnd > booking.BookingDateTime && bookingEnd <= bookedEnd))
                {
                    return BadRequest(new { message = "Time Slot Already Booked" });
                }
            }

            // Step 5: Generate Booking Token
            var dailyBookings = existingBookings.Count();
            var token = dailyBookings + 1;

            // Step 6: Add New Booking
            var bookingDetails = new BookingDetails
            {
                BookingToken = token,
                BookingStatus = "Booking Confirmed",
                BookingDateTime = request.BookingDateTime,
                PatientId = patient.PatientId,
                DoctorId = request.DoctorId,
                IsBookingCancelled = false // Default value for new booking
            };

            _context.BookingDetails.Add(bookingDetails);
            await _context.SaveChangesAsync();

            // Prepare the response DTO
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == request.DoctorId);
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == doctor.DepartmentId);

            // Saving Booking History
            var bookingHistory = new BookingHistory
            {
                BookedDate = bookingDetails.BookingDateTime,
                BookingId = bookingDetails.BookingId,
                PatientId = patient.PatientId,
                DoctorId = doctor?.DoctorId
            };

            _context.BookingHistories.Add(bookingHistory);
            await _context.SaveChangesAsync();

            // Saving the patient history
            var patientHistory = new PatientHistory
            {
                UserId = request.UserId,
                PatientId = patient.PatientId,
                ConsultedDoctor = doctor?.DoctorName,
                DoctorId = doctor?.DoctorId,
                BookingHistoryId = bookingHistory.BookingHistoryId,
                BookingId = bookingDetails.BookingId,
                ConsultedDate = bookingDetails.BookingDateTime
            };

            _context.PatientHistories.Add(patientHistory);
            await _context.SaveChangesAsync();

            var response = new BookingResponseDto
            {
                BookingToken = token,
                BookingStatus = "Booking Confirmed",
                BookingDateTime = request.BookingDateTime,
                PatientName = patient.PatientName,
                DoctorName = doctor?.DoctorName,
                Department = department?.DepartmentName
            };

            return Ok(response);
        }


        [HttpGet("getBookingDetails")]
        public async Task<IActionResult> GetBookingDetails(int userId)
        {
            var patient = await _context.PatientDetails
                .Include(p => p.BookingDetails)
                    .ThenInclude(b => b.BookingHistory)
                .Where(p => p.UserId == userId)
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound("No patient found for the specified user ID.");
            }

            // Filter the patient's bookings to only include confirmed bookings
            var confirmedBookings = patient?.BookingDetails
                .Where(b => b.BookingStatus == "Booking Confirmed")
                .ToList();

            if (!confirmedBookings.Any())
            {
                return NotFound("No confirmed bookings found for this patient.");
            }

            // Prepare the response model
            var response = confirmedBookings.Select(b => new BookingForDoctorResponseDto
            {
                BookingDetails = new BookingDetailsDto
                {
                    BookingId = b.BookingId,
                    BookingToken = b.BookingToken,
                    BookingStatus = b.BookingStatus,
                    BookingDateTime = b.BookingDateTime,
                    IsBookingCancelled = b.IsBookingCancelled
                },
                PatientDetails = new PatientDetailsDto
                {
                    PatientId = patient.PatientId,
                    PatientName = patient.PatientName,
                    PatientDescription = patient.PatientDescription,
                    PatientHealthCondition = patient.PatientHealthCondition
                },
                BookingHistory = b.BookingHistory.Select(bh => new BookingHistoryDto
                {
                    HistoryId = bh.BookingHistoryId,
                    BookedDate = bh.BookedDate
                }).ToList()
            }).ToList();

            return Ok(response);
        }


        [HttpGet("getBookingDetailsWithDoctor")]
        public async Task<IActionResult> GetBookingDetailsWithDoctor(int userId,int doctorId)
        {
            var patient = await _context.PatientDetails
                .Include(p => p.BookingDetails)
                .Include(p => p.PatientHistory)
                    .ThenInclude(b => b.BookingHistory)
                .Where(p => p.UserId == userId)
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound("No patient found for the specified user ID.");
            }

            // Filter the patient's bookings to only include confirmed bookings
            var confirmedBookings = patient?.BookingDetails
                .Where(b => b.BookingStatus == "Booking Confirmed" && b.DoctorId==doctorId)
                .ToList();

            if (!confirmedBookings.Any())
            {
                return NotFound("No confirmed bookings found for this patient.");
            }

            // Prepare the response model
            var response = confirmedBookings.Select(b => new BookingForDoctorResponseDto
            {
                BookingDetails = new BookingDetailsDto
                {
                    BookingId = b.BookingId,
                    BookingToken = b.BookingToken,
                    BookingStatus = b.BookingStatus,
                    BookingDateTime = b.BookingDateTime,
                    IsBookingCancelled = b.IsBookingCancelled
                },
                PatientDetails = new PatientDetailsDto
                {
                    PatientId = patient.PatientId,
                    PatientName = patient.PatientName,
                    PatientDescription = patient.PatientDescription,
                    PatientHealthCondition = patient.PatientHealthCondition
                },
                BookingHistory = b.BookingHistory.Select(bh => new BookingHistoryDto
                {
                    HistoryId = bh.BookingHistoryId,
                    BookedDate = bh.BookedDate
                }).ToList()
            }).ToList();

            return Ok(response);
        }
        



    }

}
