using ClinicalManagementAPI.Data;
using ClinicalManagementAPI.DataModels.RequestModels;
using ClinicalManagementAPI.Models.Doctors;
using ClinicalManagementAPI.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace ClinicalManagementAPI.Services.DoctorLogicService
{
    public interface IDoctorLogicService
    {
        Task<DepartmentDetails> AddOrUpdateDepartment(AssignDoctorRequest request);
        Task<DoctorDetails> AddOrUpdateDoctor(AssignDoctorRequest request, UserDetails user, DepartmentDetails department);

        Task UpdateUserRole(AssignDoctorRequest request);
    }
    public class DoctorLogicService:IDoctorLogicService
    {
        private readonly ClinicContext _context;

        public DoctorLogicService(ClinicContext context) {
            _context = context;
        }

        public async Task<DepartmentDetails> AddOrUpdateDepartment(AssignDoctorRequest request)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentName == request.DepartmentName || d.DepartmentId == request.DepartmentId);

            if (department == null)
            {
                department = new DepartmentDetails
                {
                    DepartmentName = request.DepartmentName,
                    DepartmentDescription = request.DepartmentDescription
                };
                _context.Departments.Add(department);
            }
            else
            {
                department.DepartmentName = request.DepartmentName;
                department.DepartmentDescription = request.DepartmentDescription;
                _context.Departments.Update(department);
            }

            await _context.SaveChangesAsync();
            return department;
        }

        public async Task<DoctorDetails> AddOrUpdateDoctor(AssignDoctorRequest request, UserDetails user, DepartmentDetails department)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == request.UserId);

            if (doctor == null)
            {
                doctor = new DoctorDetails
                {
                    UserId = user.Id,
                    DoctorName = user.Name,
                    DoctorEducation = request.DoctorEducation,
                    Specialization = request.Specialization,
                    TotalYearExperience = request.TotalYearExperience,
                    DepartmentId = department.DepartmentId
                };
                _context.Doctors.Add(doctor);
            }
            else
            {
                doctor.DoctorName = user.Name;
                doctor.DoctorEducation = request.DoctorEducation;
                doctor.Specialization = request.Specialization;
                doctor.TotalYearExperience = request.TotalYearExperience;
                doctor.DepartmentId = department.DepartmentId;
                _context.Doctors.Update(doctor);
            }

            await _context.SaveChangesAsync();
            return doctor;
        }

        public async Task UpdateUserRole(AssignDoctorRequest request)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == request.UserId);

            if (userRole != null)
            {
                userRole.UserRoleName = "Doctor";
                userRole.UserRoleNameId = 2;
                await _context.SaveChangesAsync();
            }
        }
    }
}
