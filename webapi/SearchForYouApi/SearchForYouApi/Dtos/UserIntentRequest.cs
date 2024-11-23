using System.ComponentModel.DataAnnotations;

namespace SearchForYouApi.Dtos;

public class UserIntentRequest
{
    [Required]
    public string Question { get; set; }
    
    public string ImageUrl { get; set; }
}