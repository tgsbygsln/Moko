using AirFastNew.Data;
using AirFastNew.Models;
using Microsoft.EntityFrameworkCore;

public class RecommendationService
{
    private readonly AirFastDbContext _context;

    public RecommendationService(AirFastDbContext context)
    {
        _context = context;
    }

    public async Task<List<Post>> GetRecommendedPosts(string userId)
    {
        var lastViewed = await _context.UserViewedPosts
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.ViewedAt)
            .Select(v => v.PostId)
            .FirstOrDefaultAsync();

        var referencePost = await _context.Posts.FindAsync(lastViewed);

        if (referencePost == null) return new List<Post>();

        return await _context.Posts
            .Where(p => p.Id != referencePost.Id &&
                       (
                           p.Location == referencePost.Location ||
                           p.RoomCount == referencePost.RoomCount ||
                           (p.SquareMeters >= referencePost.SquareMeters - 10 &&
                            p.SquareMeters <= referencePost.SquareMeters + 10) ||
                           (p.Price >= referencePost.Price * 0.8m &&
                            p.Price <= referencePost.Price * 1.2m)
                       ))
            .OrderByDescending(p => p.CreatedDate)
            .Take(5)
            .ToListAsync();
    }
}