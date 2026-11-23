using Microsoft.Owin;
using Owin;
using DoctorAppointmentSystem.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

[assembly: OwinStartup(typeof(DoctorAppointmentSystem.Startup))]

namespace DoctorAppointmentSystem
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            SeedRolesAndUsers();
        }

        private void SeedRolesAndUsers()
        {
            using (var context = new ApplicationDbContext())
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                // =================== ROLES ===================
                string[] roles = { "Admin", "Doctor", "User" };
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExists(role))
                        roleManager.Create(new IdentityRole(role));
                }

                // =================== ADMIN ===================
                var admin = userManager.FindByEmail("admin@gmail.com");
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = "admin@gmail.com",
                        Email = "admin@gmail.com",
                        EmailConfirmed = true
                    };
                    userManager.Create(admin, "Admin@123");
                    userManager.AddToRole(admin.Id, "Admin");
                }

                // =================== DOCTOR ===================
                var doctor = userManager.FindByEmail("doctor@gmail.com");
                if (doctor == null)
                {
                    doctor = new ApplicationUser
                    {
                        UserName = "doctor@gmail.com",
                        Email = "doctor@gmail.com",
                        EmailConfirmed = true
                    };
                    userManager.Create(doctor, "Doctor@123");
                    userManager.AddToRole(doctor.Id, "Doctor");

                    // Optional: Add Doctor details in Doctor table
                    //var doctorDetails = new Doctor
                    //{
                    //    Name = "Dr. William",
                    //    UserId = doctor.Id,
                    //    RegistrationNo = "DOC12345"
                    //};
                    //context.Doctors.Add(doctorDetails);
                    //context.SaveChanges();
                }
            }
        }
    }
}