﻿using ClinicalManagementAPI.Data;
using ClinicalManagementAPI.DataModels.RequestModels;
using ClinicalManagementAPI.DataModels.ResponseModels;
using ClinicalManagementAPI.Models.Bookings;
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
                .Where(b => b.DoctorId == doctorId && b.BookingDateTime.Value.Date == bookingDateTimeUtc)
                .ToListAsync();

            if (bookings == null || !bookings.Any())
            {
                return NotFound("No bookings found for the specified doctor on this date.");
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

            // Step 2: Check Doctor Availability for given Date and Time
            var doctorAvailability = await _context.DoctorAvailabilities
                .Where(a => a.DoctorId == request.DoctorId && a.DayOfWeek == dayOfWeek)
                .FirstOrDefaultAsync();

            if (doctorAvailability == null ||
                request.BookingDateTime.TimeOfDay < doctorAvailability.StartTime ||
                request.BookingDateTime.TimeOfDay > doctorAvailability.EndTime)
                return BadRequest("Doctor not available at this time.");

            // Step 3: Check for Existing Bookings that are not cancelled
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
                    // If all time slots are booked, we can still create a cancelled booking
                    return BadRequest("Time slot already booked.");
                }
            }

            // Step 4: Generate Booking Token
            var dailyBookings = existingBookings.Count();
            var token = dailyBookings + 1;

            // Step 5: Add New Booking
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
                ConsultedDate= bookingDetails.BookingDateTime
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

    }

}