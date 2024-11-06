using ClinicalManagementAPI.DataModels.ResponseModels;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace ClinicalManagementAPI.Services.PdfServices
{
    public interface IPdfLogicService
    {
        MemoryStream GeneratePdf(PatientBookingResponseDto response);
        string SavePdfToFile(PatientBookingResponseDto response);
    }
    public class PdfLogicService:IPdfLogicService
    {


        public PdfLogicService() {
        
        
        }


        public MemoryStream GeneratePdf(PatientBookingResponseDto response)
        {
            var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 10, 10, 10, 10);
            PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Add Title
            var titleFont = FontFactory.GetFont("Arial", 16, Font.BOLD);
            document.Add(new Paragraph("Patient Booking Details", titleFont));
            document.Add(new Paragraph("\n"));

            // Add Patient Details
            document.Add(new Paragraph($"Patient Name: {response.PatientDetails.PatientName}"));
            document.Add(new Paragraph($"Date of Birth: {response.PatientDetails.Dob}"));
            document.Add(new Paragraph($"Address: {response.PatientDetails.Address}"));
            document.Add(new Paragraph("\n"));

            // Add Doctor and Prescription Details
            foreach (var prescription in response.PrescriptionDetails)
            {
                document.Add(new Paragraph($"Prescription Name: {prescription.PrescriptionName}"));
                document.Add(new Paragraph($"Description: {prescription.PrescriptionDescription}"));
                document.Add(new Paragraph($"Disease Name: {prescription.DeseaseName}"));
                document.Add(new Paragraph($"Dosage (Morning): {prescription.MorningDosage} at {prescription.MorningDosageTime}"));
                document.Add(new Paragraph($"Dosage (Noon): {prescription.NoonDosage} at {prescription.NoonDosageTime}"));
                document.Add(new Paragraph($"Dosage (Night): {prescription.NightDosage} at {prescription.NightDosageTime}"));
                document.Add(new Paragraph("\n"));
            }

            document.Close();
            return memoryStream;
        }


        public string SavePdfToFile(PatientBookingResponseDto response)
        {
            // Define the directory path
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "SavedReports");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Define the file path
            string fileName = $"PatientDetails_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(directoryPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                var document = new Document(PageSize.A4, 25, 25, 25, 25);
                PdfWriter writer = PdfWriter.GetInstance(document, fileStream);
                document.Open();

                // Fonts
                var titleFont = FontFactory.GetFont("Arial", 17, Font.BOLD);
                var headerFont = FontFactory.GetFont("Arial", 13, Font.BOLD);
                var regularFont = FontFactory.GetFont("Arial", 10);
                var clinicFont = FontFactory.GetFont("Arial", 13, Font.BOLD, new BaseColor(10, 138, 220)); // #0a8adc color

                // Add Box Border
                PdfContentByte contentByte = writer.DirectContent;
                Rectangle rectangle = new Rectangle(document.PageSize);
                rectangle.Left += document.LeftMargin -10;
                rectangle.Right -= document.RightMargin -10;
                rectangle.Top -= document.TopMargin -10;
                rectangle.Bottom += document.BottomMargin -10;
                contentByte.Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
                contentByte.Stroke();

                // Add Clinic Name (Top Left) with color
                var clinicName = new Paragraph("Al-Huda Poly Clinic", clinicFont)
                {
                    Alignment = Element.ALIGN_LEFT
                };
                document.Add(clinicName);

                // Add Current Date and Time (Top Right)
                var currentDate = DateTime.Now.ToString("dd/MM/yyyy");
                var dateParagraph = new Paragraph(currentDate, regularFont)
                {
                    Alignment = Element.ALIGN_LEFT
                };
                document.Add(dateParagraph);

                // Add Title (Centered)
                var title = new Paragraph("Patient Report", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(title);
                document.Add(new Paragraph("\n"));

                // Patient Details Section
                document.Add(new Paragraph("Patient Details", headerFont));
                document.Add(new Paragraph($"Patient Name: {response.PatientDetails.PatientName}", regularFont));
                document.Add(new Paragraph($"Date of Birth: {response.PatientDetails.Dob}", regularFont));
                document.Add(new Paragraph($"Address: {response.PatientDetails.Address}", regularFont));
                document.Add(new Paragraph("\n"));

                // Doctor Details Section
                document.Add(new Paragraph("Doctor Details", headerFont));
                document.Add(new Paragraph($"Consulted Doctor: {response.PatientHistories.FirstOrDefault()?.ConsultedDoctor}", regularFont));
                document.Add(new Paragraph($"Consulted Date: {response.PatientHistories.FirstOrDefault()?.ConsultedDate}", regularFont));
                document.Add(new Paragraph("\n"));

                // Prescription Details Section
                document.Add(new Paragraph("Prescription Details", headerFont));
                foreach (var prescription in response.PrescriptionDetails)
                {
                    document.Add(new Paragraph($"Prescription Name: {prescription.PrescriptionName}", regularFont));
                    document.Add(new Paragraph($"Description: {prescription.PrescriptionDescription}", regularFont));
                    document.Add(new Paragraph($"Disease Name: {prescription.DeseaseName}", regularFont));
                    document.Add(new Paragraph($"Disease Description: {prescription.DeseaseDescription}", regularFont));
                    document.Add(new Paragraph($"Dosage (Morning): {prescription.MorningDosage} at {prescription.MorningDosageTime}", regularFont));
                    document.Add(new Paragraph($"Dosage (Noon): {prescription.NoonDosage} at {prescription.NoonDosageTime}", regularFont));
                    document.Add(new Paragraph($"Dosage (Night): {prescription.NightDosage} at {prescription.NightDosageTime}", regularFont));

                    if ((prescription.OtherDosage != null && prescription.OtherDosageTime != null) && (prescription.OtherDosage != 0 && prescription.OtherDosageTime != ""))
                    {
                        document.Add(new Paragraph($"Other Dosage: {prescription.OtherDosage} at {prescription.OtherDosageTime}", regularFont));
                    }

                    if (!string.IsNullOrEmpty(prescription.DoctorAdvices))
                    {
                        document.Add(new Paragraph($"Doctor Advices: {prescription.DoctorAdvices}", regularFont));
                    }
                    document.Add(new Paragraph("\n"));
                }

                // Signature Section
                document.Add(new Paragraph("\n\n"));
                document.Add(new Paragraph("Signature", headerFont) { Alignment = Element.ALIGN_RIGHT });
                document.Add(new Paragraph($"{response.PatientHistories.FirstOrDefault()?.ConsultedDoctor}", regularFont) { Alignment = Element.ALIGN_RIGHT });


                document.Close();
            }

            return filePath;
        }


    }
}
