using DoctorAppointmentSystem.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace DoctorAppointmentSystem.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _db;

        public BaseController(ApplicationDbContext db)
        {
            _db = db;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var loginInput = User.Identity.Name;
                string displayName = loginInput;

                var patient = _db.Patients
                    .FirstOrDefault(p => p.Phone == loginInput || p.Email == loginInput);

                if (patient != null)
                {
                    displayName = patient.Name;
                }

                ViewBag.UserName = displayName;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
