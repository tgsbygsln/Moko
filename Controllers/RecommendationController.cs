using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AirFastNew.Models;

public class RecommendationController : Controller
{
    private readonly RecommendationService _recommendationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RecommendationController(RecommendationService recommendationService, UserManager<ApplicationUser> userManager)
    {
        _recommendationService = recommendationService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return BadRequest("User ID cannot be null.");
        }

        var posts = await _recommendationService.GetRecommendedPosts(userId);
        return View(posts);
    }
}