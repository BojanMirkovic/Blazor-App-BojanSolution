using System.ComponentModel.DataAnnotations;

namespace Shared_Layer.DTO_s.User
{
    public class UserIdDTO
    {
        [Required(ErrorMessage = "User ID is required.")]
        [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "The ID format is incorrect.")]
        public string Id { get; set; }
    }
}
