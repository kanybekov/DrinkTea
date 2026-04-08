using System.ComponentModel.DataAnnotations;

namespace DrinkTea.Shared.Models.Requests;

public class SaveTeaFeedbackRequest
{
    [Range(1, 5, ErrorMessage = "Оценка должна быть от 1 до 5.")]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    [MaxLength(1000)]
    public string? PrivateNote { get; set; }
}
