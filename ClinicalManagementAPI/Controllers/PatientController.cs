using Azure;
using ClinicalManagementAPI.Data;
using ClinicalManagementAPI.DataModels.RequestModels;
using ClinicalManagementAPI.DataModels.ResponseModels;
using ClinicalManagementAPI.Models.Doctors;
using ClinicalManagementAPI.Models.Prescription;
using ClinicalManagementAPI.Models.Users;
using ClinicalManagementAPI.Services.PdfServices;
using ClinicalManagementAPI.Utility.Mail;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ClinicalManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly ClinicContext _context;
        private readonly IPdfLogicService _pdfLogicService;
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailHelper _mailHelper;

        public PatientController(ClinicContext context,IPdfLogicService pdfLogicService, IMailTemplate mailTemplate, IMailHelper mailHelper)
        {
            _context = context;
            _pdfLogicService = pdfLogicService;
            _mailTemplate = mailTemplate;
            _mailHelper = mailHelper;
        }



        [HttpGet("getPatientBookingDetails")]
        public async Task<IActionResult> GetPatientBookingDetails(int patientId, int bookingId, int doctorId)
        {
            // Fetch patient details and include User details for Dob, Gender, and Address
            var patient = await _context.PatientDetails
                .Include(p => p.PatientHistory)
                .Include(p => p.PrescriptionDetails) // Include prescriptions for this patient
                .Include(p => p.User) // Include User to access Dob, Gender, and Address
                .Where(p => p.PatientId == patientId)
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound("There's no patient details for this patient");
            }

            // Filter patient history based on the specified doctorId
            var filteredPatientHistory = patient.PatientHistory
                .Where(ph => ph.DoctorId == doctorId)
                .Select(ph => new PatientResponseHistoryDto
                {
                    HistoryId = ph.HistoryId,
                    ConsultedDate = ph.ConsultedDate,
                    ConsultedDoctor = ph.ConsultedDoctor
                }).ToList();

            // Filter prescription details based on the specified doctorId
            var filteredPrescriptionDetails = patient.PrescriptionDetails
                .Where(pd => pd.DoctorId == doctorId)
                .Select(pd => new PrescriptionResponseDto
                {
                    PrescriptionId = pd.PrescriptionId,
                    PrescriptionName = pd.PrescriptionName,
                    PrescriptionDescription = pd.PrescriptionDescription,
                    DeseaseName=pd.DeseaseName,
                    DeseaseDescription=pd.DeseaseDescription,
                    DeseaseType=pd.DeseaseType,
                    MorningDosage = pd.MorningDosage,
                    MorningDosageTime = pd.MorningDosageTime,
                    NoonDosage = pd.NoonDosage,
                    NoonDosageTime = pd.NoonDosageTime,
                    NightDosage = pd.NightDosage,
                    NightDosageTime = pd.NightDosageTime,
                    OtherDosage = pd.OtherDosage,
                    OtherDosageTime = pd.OtherDosageTime,
                    DoctorAdvices=pd.DoctorAdvices,
                    BookingId = pd.BookingId,
                    PrescribedDate = pd.PrescribedDate
                }).ToList();

            // Fetch the booking details for the specified booking ID and doctor ID
            var booking = await _context.BookingDetails
                .Include(b => b.BookingHistory)
                .Where(b => b.BookingId == bookingId && b.DoctorId == doctorId)
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                return NotFound("No booking found for the specified Patient and for this doctor");
            }

            // Prepare the response model
            var response = new PatientBookingResponseDto
            {
                PatientDetails = new PatientResponseDetailsDto
                {
                    PatientId = patient.PatientId,
                    PatientName = patient.PatientName,
                    PatientDescription = patient.PatientDescription,
                    PatientHealthCondition = patient.PatientHealthCondition,
                    Gender = patient.User?.Gender,
                    Dob = patient.User?.Dob,
                    Address = patient.User?.Address,
                    PhoneNumber=patient.User?.Phone,
                    EmailAddress=patient.User?.EmailAddress
                },
                PatientHistories = filteredPatientHistory, // Only history matching the doctorId
                PrescriptionDetails = filteredPrescriptionDetails, // Only prescriptions matching the doctorId
                BookingDetails = new PatientBookingDetailsDto
                {
                    BookingId = booking.BookingId,
                    BookingToken = booking.BookingToken,
                    BookingStatus = booking.BookingStatus,
                    BookingDateTime = booking.BookingDateTime,
                    IsBookingCancelled = booking.IsBookingCancelled
                },
                BookingHistories = booking.BookingHistory.Select(bh => new PatientBookingHistoryDto
                {
                    BookingHistoryId = bh.BookingHistoryId,
                    BookedDate = bh.BookedDate
                }).ToList()
            };

            return Ok(response);
        }


        [HttpGet("getPatientBookings")]
        public async Task<IActionResult> GetPatientBookings(int userId, [FromQuery] string bookingStatus = null)
        {
            var patientData = await _context.PatientDetails
                .Where(p => p.UserId == userId)
                .Select(p => new PatientBookingsResponseDto
                {
                    Patient = new PatientBookingsResponseDto.PatientDto
                    {
                        PatientId = p.PatientId,
                        PatientGuid = p.PatientGuid.ToString(),
                        PatientName = p.PatientName,
                        PatientDescription = p.PatientDescription,
                        PatientHealthCondition = p.PatientHealthCondition
                    },
                    Bookings = p.BookingDetails
                        .Where(b => bookingStatus == null || b.BookingStatus == bookingStatus)
                        .Select(b => new PatientBookingsResponseDto.BookingDto
                        {
                            BookingId = b.BookingId,
                            BookingToken = b.BookingToken,
                            BookingStatus = b.BookingStatus,
                            BookingDateTime = b.BookingDateTime,
                            Doctor = new PatientBookingsResponseDto.DoctorDto
                            {
                                DoctorId = b.DoctorDetails.DoctorId,
                                DoctorGuid = b.DoctorDetails.DoctorGuid.ToString(),
                                DoctorName = b.DoctorDetails.DoctorName,
                                DoctorEducation = b.DoctorDetails.DoctorEducation,
                                Specialization = b.DoctorDetails.Specialization,
                                TotalYearExperience = b.DoctorDetails.TotalYearExperience,
                                Department = new PatientBookingsResponseDto.DepartmentDto
                                {
                                    DepartmentId = b.DoctorDetails.Department.DepartmentId,
                                    DepartmentName = b.DoctorDetails.Department.DepartmentName,
                                    DepartmentDescription = b.DoctorDetails.Department.DepartmentDescription
                                }
                            },
                            BookingHistories = b.BookingHistory.Select(h => new PatientBookingsResponseDto.BookingHistoryDto
                            {
                                BookingHistoryId = h.BookingHistoryId,
                                BookedDate = h.BookedDate
                            }).ToList()
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (patientData == null || !patientData.Bookings.Any())
            {
                return NotFound("No bookings found for this patient.");
            }

            return Ok(patientData);
        }




        [HttpGet("generatePatientReport")]
        public async Task<IActionResult> GeneratePatientReport(int patientId, int bookingId, int doctorId)
        {
            // Fetch patient details and include User details for Dob, Gender, and Address
            var patient = await _context.PatientDetails
                .Include(p => p.PatientHistory)
                .Include(p => p.PrescriptionDetails) // Include prescriptions for this patient
                .Include(p => p.User) // Include User to access Dob, Gender, and Address
                .Where(p => p.PatientId == patientId)
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound("There's no patient details for this patient");
            }

            // Filter patient history based on the specified doctorId
            var filteredPatientHistory = patient.PatientHistory
                .Where(ph => ph.DoctorId == doctorId)
                .Select(ph => new PatientResponseHistoryDto
                {
                    HistoryId = ph.HistoryId,
                    ConsultedDate = ph.ConsultedDate,
                    ConsultedDoctor = ph.ConsultedDoctor
                }).ToList();

            // Filter prescription details based on the specified doctorId
            var filteredPrescriptionDetails = patient.PrescriptionDetails
                .Where(pd => pd.DoctorId == doctorId)
                .Select(pd => new PrescriptionResponseDto
                {
                    PrescriptionId = pd.PrescriptionId,
                    PrescriptionName = pd.PrescriptionName,
                    PrescriptionDescription = pd.PrescriptionDescription,
                    DeseaseName = pd.DeseaseName,
                    DeseaseDescription = pd.DeseaseDescription,
                    DeseaseType = pd.DeseaseType,
                    MorningDosage = pd.MorningDosage,
                    MorningDosageTime = pd.MorningDosageTime,
                    NoonDosage = pd.NoonDosage,
                    NoonDosageTime = pd.NoonDosageTime,
                    NightDosage = pd.NightDosage,
                    NightDosageTime = pd.NightDosageTime,
                    OtherDosage = pd.OtherDosage,
                    OtherDosageTime = pd.OtherDosageTime,
                    DoctorAdvices = pd.DoctorAdvices,
                    PrescribedDate=pd.PrescribedDate
                }).ToList();

            // Fetch the booking details for the specified booking ID and doctor ID
            var booking = await _context.BookingDetails
                .Include(b => b.BookingHistory)
                .Where(b => b.BookingId == bookingId && b.DoctorId == doctorId)
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                return NotFound("No booking found for the specified Patient and for this doctor");
            }

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == doctorId);
            // Prepare the response model
            var response = new PatientBookingResponseDto
            {
                PatientDetails = new PatientResponseDetailsDto
                {
                    PatientId = patient.PatientId,
                    PatientName = patient.PatientName,
                    PatientDescription = patient.PatientDescription,
                    PatientHealthCondition = patient.PatientHealthCondition,
                    Gender = patient.User?.Gender,
                    Dob = patient.User?.Dob,
                    Address = patient.User?.Address,
                    PhoneNumber = patient.User?.Phone,
                    EmailAddress = patient.User?.EmailAddress
                },
                PatientHistories = filteredPatientHistory, // Only history matching the doctorId
                PrescriptionDetails = filteredPrescriptionDetails, // Only prescriptions matching the doctorId
                BookingDetails = new PatientBookingDetailsDto
                {
                    BookingId = booking.BookingId,
                    BookingToken = booking.BookingToken,
                    BookingStatus = booking.BookingStatus,
                    BookingDateTime = booking.BookingDateTime,
                    IsBookingCancelled = booking.IsBookingCancelled
                },
                BookingHistories = booking.BookingHistory.Select(bh => new PatientBookingHistoryDto
                {
                    BookingHistoryId = bh.BookingHistoryId,
                    BookedDate = bh.BookedDate
                }).ToList()
            };

            var pdfBytes = await _pdfLogicService.GeneratePdf(response);
            var base64Pdf = Convert.ToBase64String(pdfBytes);

            // Return the Base64-encoded PDF as JSON
            return Ok(new { PdfContent = base64Pdf });
        }

        [HttpGet("generateSpecificPatientReport")]
        public async Task<IActionResult> GenerateSpecificPatientReport(int patientId, int bookingId, int doctorId)
        {
            // Fetch patient details and include User details for Dob, Gender, and Address
            var patient = await _context.PatientDetails
                .Include(p => p.PatientHistory)
                .Include(p => p.PrescriptionDetails) // Include prescriptions for this patient
                .Include(p => p.User) // Include User to access Dob, Gender, and Address
                .Where(p => p.PatientId == patientId)
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound("No patient details found for this patient.");
            }

            // Filter patient history based on the specified doctorId
            var filteredPatientHistory = patient.PatientHistory
                .Where(ph => ph.DoctorId == doctorId)
                .Select(ph => new PatientResponseHistoryDto
                {
                    HistoryId = ph.HistoryId,
                    ConsultedDate = ph.ConsultedDate,
                    ConsultedDoctor = ph.ConsultedDoctor
                }).ToList();

            // Filter prescription details based on the specified doctorId and bookingId
            var filteredPrescriptionDetails = patient.PrescriptionDetails
                .Where(pd => pd.DoctorId == doctorId && pd.BookingId == bookingId)
                .Select(pd => new PrescriptionResponseDto
                {
                    PrescriptionId = pd.PrescriptionId,
                    PrescriptionName = pd.PrescriptionName,
                    PrescriptionDescription = pd.PrescriptionDescription,
                    DeseaseName = pd.DeseaseName,
                    DeseaseDescription = pd.DeseaseDescription,
                    DeseaseType = pd.DeseaseType,
                    MorningDosage = pd.MorningDosage,
                    MorningDosageTime = pd.MorningDosageTime,
                    NoonDosage = pd.NoonDosage,
                    NoonDosageTime = pd.NoonDosageTime,
                    NightDosage = pd.NightDosage,
                    NightDosageTime = pd.NightDosageTime,
                    OtherDosage = pd.OtherDosage,
                    OtherDosageTime = pd.OtherDosageTime,
                    DoctorAdvices = pd.DoctorAdvices,
                    PrescribedDate = pd.PrescribedDate
                }).ToList();

            // Fetch the booking details for the specified booking ID and doctor ID
            var booking = await _context.BookingDetails
                .Include(b => b.BookingHistory)
                .Where(b => b.BookingId == bookingId && b.DoctorId == doctorId)
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                return NotFound("No booking found for the specified patient and doctor.");
            }

            // Prepare the response model
            var response = new PatientBookingResponseDto
            {
                PatientDetails = new PatientResponseDetailsDto
                {
                    PatientId = patient.PatientId,
                    PatientName = patient.PatientName,
                    PatientDescription = patient.PatientDescription,
                    PatientHealthCondition = patient.PatientHealthCondition,
                    Gender = patient.User?.Gender,
                    Dob = patient.User?.Dob,
                    Address = patient.User?.Address,
                    PhoneNumber = patient.User?.Phone,
                    EmailAddress = patient.User?.EmailAddress
                },
                PatientHistories = filteredPatientHistory,
                PrescriptionDetails = filteredPrescriptionDetails,
                BookingDetails = new PatientBookingDetailsDto
                {
                    BookingId = booking.BookingId,
                    BookingToken = booking.BookingToken,
                    BookingStatus = booking.BookingStatus,
                    BookingDateTime = booking.BookingDateTime,
                    IsBookingCancelled = booking.IsBookingCancelled
                },
                BookingHistories = booking.BookingHistory.Select(bh => new PatientBookingHistoryDto
                {
                    BookingHistoryId = bh.BookingHistoryId,
                    BookedDate = bh.BookedDate
                }).ToList()
            };

            var pdfBytes = await _pdfLogicService.GeneratePdf(response);
            var base64Pdf = Convert.ToBase64String(pdfBytes);

            // Return the Base64-encoded PDF as JSON
            return Ok(new { PdfContent = base64Pdf });
        }


        [HttpPost("addPrescription")]
        public async Task<IActionResult> AddPrescription(PrescriptionRequestDto request)
        {
            // Check if the patient exists
            var patient = await _context.PatientDetails.FindAsync(request.PatientId);
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            // Check if the doctor exists
            var doctor = await _context.Doctors.FindAsync(request.DoctorId);
            if (doctor == null)
            {
                return NotFound("Doctor not found.");
            }

            // Check if the booking exists
            var booking = await _context.BookingDetails.FindAsync(request.BookingId);


            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            var patientHistory=  _context.PatientHistories.Where(p=>
            p.PatientId==request.PatientId && 
            p.DoctorId==request.DoctorId && 
            p.BookingId==request.BookingId);

            if (patientHistory == null)
            {
                return NotFound("patientHistory not found");
            }
            // Create a new PrescriptionDetails entry
            var prescription = new PrescriptionDetails
            {
                PrescriptionName = request.PrescriptionName,
                PrescriptionDescription = request.PrescriptionDescription,
                DeseaseName = request.DeseaseName,
                DeseaseDescription = request.DeseaseDescription,
                MorningDosage = request.MorningDosage,
                NoonDosage = request.NoonDosage,
                NightDosage = request.NightDosage,
                OtherDosage = request.OtherDosage,
                MorningDosageTime = request.MorningDosageTime,
                NoonDosageTime = request.NoonDosageTime,
                NightDosageTime = request.NightDosageTime,
                OtherDosageTime = request.OtherDosageTime,
                DoctorAdvices = request.DoctorAdvices,
                PrescribedDate = DateTime.UtcNow, 
                PatientId = request.PatientId,
                BookingId = request.BookingId,
                DoctorId = request.DoctorId,
            };

            // Add the prescription to the database
            _context.PrescriptionDetails.Add(prescription);
            await _context.SaveChangesAsync();

            // Create a new PrescriptionHistory entry
            var prescriptionHistory = new PrescriptionHistory
            {
                PrescribedDate = DateTime.Now,
                PrescribedDoctorName = doctor.DoctorName,  
                PrescriptionId = prescription.PrescriptionId,
                PatientId = request.PatientId,
                BookingId = request.BookingId,
                DoctorId = request.DoctorId
            };

            // Add the prescription history to the database
            _context.PrescriptionHistory.Add(prescriptionHistory);
            await _context.SaveChangesAsync();

           if(booking!=null)
            {
                // Update existing availability
                booking.BookingStatus = "Booking Completed";
                await _context.SaveChangesAsync();
            }

            

            return Ok();
        }



        [HttpPost("sendEmail")]
        public async Task <IActionResult> SendEmail(string emailAddress)
        {

            string body = "Test Email";
            var recipients = new List<string> { emailAddress };
            var validRecipients = recipients
                .Where(recipient => Regex.IsMatch(recipient, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
            .ToArray();


            _mailHelper.SendEmailInBackground(validRecipients, "Patient Report", body);

            return Ok();
        }




        [HttpGet("getPatientPrescriptions")]
        public async Task<IActionResult> GetPatientPrescriptions(int patientId, int doctorId, int? bookingId = null)
        {
            var result = await _context.PatientDetails
                .Where(p => p.PatientId == patientId)
                .Select(p => new PatientPrescriptionsResponseDto
                {
                    Patient = new PatientPrescriptionsResponseDto.PatientDto
                    {
                        PatientId = p.PatientId,
                        PatientGuid = p.PatientGuid.ToString(),
                        PatientName = p.PatientName,
                        PatientDescription = p.PatientDescription,
                        PatientHealthCondition = p.PatientHealthCondition,
                        // Calculate UserAlreadyRate property
                        UserAlreadyRate = _context.UserRatings.Any(ur =>
                            ur.PatientId == patientId &&
                            ur.DoctorId == doctorId &&
                            (bookingId == null || ur.BookingId == bookingId))
                    },
                    Doctor = p.PrescriptionDetails
                        .Where(pr => pr.DoctorId == doctorId)
                        .Select(pr => new PatientPrescriptionsResponseDto.DoctorDto
                        {
                            DoctorId = pr.DoctorDetails.DoctorId,
                            DoctorGuid = pr.DoctorDetails.DoctorGuid.ToString(),
                            DoctorName = pr.DoctorDetails.DoctorName,
                            DoctorEducation = pr.DoctorDetails.DoctorEducation,
                            Specialization = pr.DoctorDetails.Specialization,
                            TotalYearExperience = pr.DoctorDetails.TotalYearExperience,
                            Department = new PatientPrescriptionsResponseDto.DepartmentDto
                            {
                                DepartmentId = pr.DoctorDetails.Department.DepartmentId,
                                DepartmentName = pr.DoctorDetails.Department.DepartmentName,
                                DepartmentDescription = pr.DoctorDetails.Department.DepartmentDescription
                            }
                        })
                        .FirstOrDefault(),
                    Prescriptions = p.PrescriptionDetails
                        .Where(pr => pr.DoctorId == doctorId && (bookingId == null || pr.BookingId == bookingId))
                        .Select(pr => new PatientPrescriptionsResponseDto.PrescriptionDto
                        {
                            PrescriptionId = pr.PrescriptionId,
                            PrescriptionName = pr.PrescriptionName,
                            PrescriptionDescription = pr.PrescriptionDescription,
                            DeseaseName = pr.DeseaseName,
                            DeseaseType = pr.DeseaseType,
                            DeseaseDescription = pr.DeseaseDescription,
                            MorningDosage = pr.MorningDosage,
                            NoonDosage = pr.NoonDosage,
                            NightDosage = pr.NightDosage,
                            OtherDosage = pr.OtherDosage,
                            MorningDosageTime = pr.MorningDosageTime,
                            NoonDosageTime = pr.NoonDosageTime,
                            NightDosageTime = pr.NightDosageTime,
                            OtherDosageTime = pr.OtherDosageTime,
                            DoctorAdvices = pr.DoctorAdvices,
                            PrescribedDate = pr.PrescribedDate,
                            BookingDetails = pr.BookingDetails != null ? new PatientPrescriptionsResponseDto.BookingDetailsDto
                            {
                                BookingId = pr.BookingDetails.BookingId,
                                BookingToken = pr.BookingDetails.BookingToken,
                                BookingStatus = pr.BookingDetails.BookingStatus,
                                BookingDateTime = pr.BookingDetails.BookingDateTime,
                                IsBookingCancelled = pr.BookingDetails.IsBookingCancelled
                            } : null
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound("No prescriptions found for this patient and doctor.");
            }

            return Ok(result);
        }


        [HttpPost("rate")]
        public async Task<IActionResult> Rate([FromBody] UserRatingsRequestDto ratingsRequest)
        {
            if (ratingsRequest == null)
                return BadRequest("Invalid rating request");

            // Fetch patient details for PatientWhoRated
            var patient = await _context.PatientDetails
                .FirstOrDefaultAsync(p => p.PatientId == ratingsRequest.PatientId);

            if (patient == null)
                return NotFound("Patient not found");

            // Fetch doctor details if DoctorId is provided
            DoctorDetails? doctor = null;
            if (ratingsRequest.DoctorId.HasValue)
            {
                doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.DoctorId == ratingsRequest.DoctorId.Value);

                if (doctor == null)
                    return NotFound("Doctor not found");
            }

            // Fetch booking details for validation
            var booking = await _context.BookingDetails
                .FirstOrDefaultAsync(b => b.BookingId == ratingsRequest.BookingId);

            if (booking == null)
                return NotFound("Booking not found");

            // Create UserRatings entry
            var userRatings = new UserRatings
            {
                PatientId = ratingsRequest.PatientId,
                BookingId = ratingsRequest.BookingId,
                PatientWhoRated = patient.PatientName,
                RatedDoctor = doctor?.DoctorName,
                DoctorRatingsValue = ratingsRequest.DoctorRatingsValue,
                PatientFeedbackForDoctor = ratingsRequest.PatientFeedbackForDoctor,
                PatientFeedbackForClinic = ratingsRequest.PatientFeedbackForClinic,
                ClinicRatingValue = ratingsRequest.ClinicRatingValue,
                DoctorId = ratingsRequest.DoctorId
            };

            // Add and save the rating
            _context.UserRatings.Add(userRatings);
            await _context.SaveChangesAsync();

            return Ok();
        }



        [HttpGet("ratings/doctor")]
        public async Task<IActionResult> GetDoctorRatings(int doctorId)
        {
            // Check if doctor exists
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            if (doctor == null)
                return NotFound("Doctor not found");

            // Fetch ratings for the doctor with booking details
            var doctorRatings = await _context.UserRatings
                .Where(r => r.DoctorId == doctorId && r.DoctorRatingsValue.HasValue)
                .Select(r => new
                {
                    r.UserRatingsId,
                    r.PatientWhoRated,
                    r.DoctorRatingsValue,
                    r.PatientFeedbackForDoctor,
                    r.BookingId,
                    BookingDetails = new
                    {
                        r.BookingDetails.BookingId,
                        r.BookingDetails.BookingToken,
                        r.BookingDetails.BookingStatus,
                        r.BookingDetails.BookingDateTime,
                        r.BookingDetails.IsBookingCancelled,
                        PatientName = r.BookingDetails.PatientDetails.PatientName,
                        DoctorName = r.BookingDetails.DoctorDetails.DoctorName
                    }
                })
                .ToListAsync();

            return Ok(doctorRatings);
        }

        
        
        [HttpGet("ratings/clinic")]
        public async Task<IActionResult> GetClinicRatings()
        {
            // Fetch ratings for the clinic with booking details
            var clinicRatings = await _context.UserRatings
                .Where(r => r.ClinicRatingValue.HasValue)
                .Select(r => new
                {
                    r.UserRatingsId,
                    r.PatientWhoRated,
                    r.ClinicRatingValue,
                    r.PatientFeedbackForClinic,
                    r.BookingId,
                    BookingDetails = new
                    {
                        r.BookingDetails.BookingId,
                        r.BookingDetails.BookingToken,
                        r.BookingDetails.BookingStatus,
                        r.BookingDetails.BookingDateTime,
                        r.BookingDetails.IsBookingCancelled,
                        PatientName = r.BookingDetails.PatientDetails.PatientName,
                        DoctorName = r.BookingDetails.DoctorDetails.DoctorName
                    }
                })
                .ToListAsync();

            return Ok(clinicRatings);
        }




    }
}
