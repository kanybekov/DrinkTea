using DrinkTea.BL.Interfaces;
using DrinkTea.Shared.Models.Requests;
using DrinkTea.Shared.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrinkTea.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeasController(ITeaService teaService) : ControllerBase
{
    /// <summary>
    /// 	Получить полный список чая с остатками и ценами для инвентаризации.
    /// </summary>
    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventory()
    {
        var rawData = await teaService.GetFullInventoryAsync();

        // Маппим динамические данные в наш типизированный Response
        var result = rawData.Select(x => new TeaInventoryResponse(
            (Guid)x.id,
            (string)x.name,
            (decimal)x.currentstock,
            (decimal)(x.brewprice ?? 0m), // Если цена не задана, вернем 0
            (decimal)(x.saleprice ?? 0m)
        ));

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeaRequest req)
    {
        var id = await teaService.CreateTeaWithPriceAsync(req.Name, req.InitialStock, req.BrewPrice, req.SalePrice);
        return Ok(new { TeaId = id });
    }

    [HttpPost("{id:guid}/restock")]
    public async Task<IActionResult> Restock(Guid id, [FromBody] RestockRequest req)
    {
        await teaService.RestockAsync(id, req.Amount, req.NewBrewPrice, req.NewSalePrice);
        return Ok(new { Message = "Склад обновлен" });
    }

    [HttpPatch("{id:guid}/prices")]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> UpdatePrices(Guid id, [FromBody] UpdateTeaPricesRequest req)
    {
        // Вызываем метод сервиса (его нужно будет добавить в TeaService)
        await teaService.UpdateTeaPricesAsync(id, req.BrewPrice, req.SalePrice);
        return Ok(new { Message = "Цены обновлены" });
    }

    [HttpGet("{id:guid}/feedback")]
    [Authorize]
    public async Task<IActionResult> GetFeedback(Guid id, [FromServices] UserContext userContext)
    {
        var tea = await teaService.GetTeaWithFeedbackAsync(id, userContext.UserId);
        if (tea == null)
        {
            return NotFound();
        }

        var ratingsCount = tea.PublicReviews.Count;
        var averageRating = ratingsCount == 0 ? 0 : Math.Round(tea.PublicReviews.Average(x => (decimal)x.Rating), 2);

        var response = new TeaFeedbackResponse
        {
            TeaId = tea.Id,
            TeaName = tea.Name,
            AverageRating = averageRating,
            RatingsCount = ratingsCount,
            PublicReviews = tea.PublicReviews.Select(x => new PublicReviewDto
            {
                Id = x.Id,
                Rating = x.Rating,
                Comment = x.Comment,
                UserId = x.UserId,
                UserName = x.UserName,
                CreatedAt = x.CreatedAt
            }).ToList(),
            PrivateNotes = tea.PrivateNotes.Select(x => new PrivateNoteDto
            {
                NoteText = x.NoteText,
                UserId = x.UserId,
                CreatedAt = x.CreatedAt
            }).ToList()
        };

        return Ok(response);
    }

    [HttpPost("{id:guid}/feedback")]
    [Authorize]
    public async Task<IActionResult> SaveFeedback(Guid id, [FromBody] SaveTeaFeedbackRequest req, [FromServices] UserContext userContext)
    {
        if (req.Rating is < 1 or > 5)
        {
            return BadRequest(new { Message = "Оценка должна быть от 1 до 5." });
        }

        await teaService.SaveFeedbackAsync(id, userContext.UserId, req.Rating, req.Comment, req.PrivateNote);
        return Ok(new { Message = "Отзыв сохранен" });
    }

    [HttpDelete("{id:guid}/feedback")]
    [Authorize]
    public async Task<IActionResult> DeleteMyFeedback(Guid id, [FromServices] UserContext userContext)
    {
        var deleted = await teaService.DeleteMyReviewAsync(id, userContext.UserId);
        return deleted ? Ok(new { Message = "Отзыв удален" }) : NotFound();
    }

    [HttpDelete("{id:guid}/reviews/{reviewId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(Guid id, Guid reviewId, [FromServices] UserContext userContext)
    {
        if (userContext.Role != DrinkTea.Shared.Enums.UserRoles.Master)
        {
            return Forbid();
        }

        var deleted = await teaService.DeleteReviewByIdAsync(id, reviewId);
        return deleted ? Ok(new { Message = "Отзыв удален администратором" }) : NotFound();
    }

    [HttpGet("ratings")]
    [Authorize]
    public async Task<IActionResult> GetRatings([FromServices] UserContext userContext)
    {
        var teas = await teaService.GetTeasForRatingsAsync(userContext.UserId);
        var result = teas
            .Select(x =>
            {
                var ratingsCount = x.PublicReviews.Count;
                var avg = ratingsCount == 0 ? 0 : Math.Round(x.PublicReviews.Average(r => (decimal)r.Rating), 2);

                return new TeaRatingListItemResponse
                {
                    TeaId = x.Id,
                    TeaName = x.Name,
                    AverageRating = avg,
                    RatingsCount = ratingsCount,
                    RecentPublicComments = x.PublicReviews
                        .Where(r => r.UserId != userContext.UserId)
                        .OrderByDescending(r => r.CreatedAt)
                        .Take(3)
                        .Select(r => new PublicReviewDto
                        {
                            Rating = r.Rating,
                            Comment = r.Comment,
                            UserId = r.UserId,
                            UserName = r.UserName,
                            CreatedAt = r.CreatedAt
                        })
                        .ToList()
                };
            })
            .OrderByDescending(x => x.AverageRating)
            .ThenByDescending(x => x.RatingsCount)
            .ThenBy(x => x.TeaName)
            .ToList();

        return Ok(result);
    }

    [HttpGet("my-ratings")]
    [Authorize]
    public async Task<IActionResult> GetMyRatings([FromServices] UserContext userContext)
    {
        var result = await teaService.GetMyTeaRatingsAsync(userContext.UserId);
        return Ok(result);
    }
}
