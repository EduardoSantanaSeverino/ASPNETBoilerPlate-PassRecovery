using System.ComponentModel.DataAnnotations;

namespace SocialUplift.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}