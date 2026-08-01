using System.ComponentModel.DataAnnotations;

namespace PosServer.Models
{
    public class LoginRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string Password { get; set; } = string.Empty;
    }
}
