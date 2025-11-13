using Microsoft.AspNetCore.Mvc;

namespace attendance.Controllers
{
    /// <summary>
    /// Root controller - provides API information
    /// </summary>
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        /// <summary>
        /// Get API information
        /// GET: /
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new
            {
                message = "Attendance Management System API",
                version = "1.0.0",
                status = "Running",
                documentation = "/swagger",
                endpoints = new
                {
                    attendance = "/api/attendance",
                    leave = "/api/leave",
                    profile = "/api/profile",
                    admin = "/api/admin"
                }
            });
        }
    }
}
