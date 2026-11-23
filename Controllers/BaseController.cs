using System.Linq;
using System.Web.Mvc;
using DoctorAppointmentSystem.Models;

namespace DoctorAppointmentSystem.Controllers
{
    public class BaseController : Controller
    {
        protected ApplicationDbContext db = new ApplicationDbContext();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.Identity.IsAuthenticated)
            {
                var loginInput = User.Identity.Name;

                // 👤 DEFAULT
                string displayName = loginInput;

                // 🧠 CHECK PATIENT FIRST (USER ROLE)
                var patient = db.Patients
                    .FirstOrDefault(p => p.Phone == loginInput || p.Email == loginInput);

                if (patient != null)
                {
                    displayName = patient.Name; // User → Name
                }
                else
                {
                    // 🩺 DOCTOR / ADMIN → show EMAIL
                    displayName = loginInput; // already email or username
                }

                ViewBag.UserName = displayName;
            }

            base.OnActionExecuting(filterContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}