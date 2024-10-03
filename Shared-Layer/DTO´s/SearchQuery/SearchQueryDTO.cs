
using System.ComponentModel.DataAnnotations;

namespace Shared_Layer.DTO_s.SearchQuery
{
    public class SearchQueryDTO
    {
        [StringLength(30, ErrorMessage = "Search query cannot exceed 30 characters.")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Search query can only contain letters and spaces.")]
        public string SearchQuery { get; set; } = string.Empty; // Default to an empty string
    }
}
