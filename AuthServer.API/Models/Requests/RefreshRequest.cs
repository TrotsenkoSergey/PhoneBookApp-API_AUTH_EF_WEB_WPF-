using System.ComponentModel.DataAnnotations;

namespace AuthServer.API.Models.Requests
{
    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
