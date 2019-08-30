using System.ComponentModel.DataAnnotations;

namespace SocialUplift.Users.Dto
{
    public class ResetPasswordEmailDto
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }
    }
}
