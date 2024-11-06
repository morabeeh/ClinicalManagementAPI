using ClinicalManagementAPI.Data;
using ClinicalManagementAPI.DataModels.RequestModels;
using ClinicalManagementAPI.DataModels.ResponseModels;
using ClinicalManagementAPI.Models.Doctors;
using ClinicalManagementAPI.Models.Prescription;
using ClinicalManagementAPI.Services.PdfServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicalManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly ClinicContext _context;
        private readonly IPdfLogicService _pdfLogicService;

        public PatientController(ClinicContext context,IPdfLogicService pdfLogicService)
        {
            _context = context;
            _pdfLogicService = pdfLogicService;
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
                    Address = patient.User?.Address
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
                    DoctorAdvices=pd.DoctorAdvices
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
                    Address = patient.User?.Address
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

            //var pdfStream= _pdfLogicService.GeneratePdf(response);

            string filePath = _pdfLogicService.SavePdfToFile(response);

            return Ok(response);
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

    }
}
