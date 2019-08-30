using System.ComponentModel.DataAnnotations;

namespace SocialUplift.Users.Dto
{
    public class ForgotPasswordDto
    {
        [Required]
        public string Email { get; set; }
        
    }
}
