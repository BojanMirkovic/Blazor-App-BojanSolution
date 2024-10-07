
using System.ComponentModel.DataAnnotations;

namespace Shared_Layer.DTO_s.User
{
    public class UserEmailDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }
    }
}
