using System.ComponentModel.DataAnnotations;

namespace AuthServer.API.Controllers
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
