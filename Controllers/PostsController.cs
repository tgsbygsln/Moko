using Microsoft.AspNetCore.Mvc;
using AirFastNew.Models;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AirFastNew.Controllers
{
    [Authorize] 
    public class PostsController : Controller
    {
        private readonly AirFastDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PostsController> _logger;
        private readonly RecommendationService _recommendationService;

        public PostsController(AirFastDbContext context, UserManager<ApplicationUser> userManager, ILogger<PostsController> logger, RecommendationService recommendationService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _recommendationService = recommendationService;
        }
        [AllowAnonymous]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            
            if (post == null)
            {
                _logger.LogError($"Post with ID {id} not found.");
                return NotFound();
            }

            // Log the user's viewed post
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                _context.UserViewedPosts.Add(new UserViewedPost
                {
                    UserId = userId,
                    PostId = id,
                    ViewedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            if (post.IsApproved == false && post.RejectReason != null)
            {
                ViewBag.RejectReason = post.RejectReason;
            }

            var latestPosts = await _context.Posts
                .Where(p => p.Id != id)  // Exclude the current post
                .OrderByDescending(p => p.CreatedDate) // Sort newest first
                .Take(4) // Limit to 8 posts
                .ToListAsync();

            var recommendedPosts = userId != null 
                ? await _recommendationService.GetRecommendedPosts(userId) 
                : new List<Post>();

            var viewModel = new PostDetailsViewModel
            {
                Post = post,
                OtherPosts = latestPosts,
                RecommendedPosts = recommendedPosts
            };

            return View(viewModel);
        }

        // Create Post
        public IActionResult Create()
        {
            return View();
        }

        // Create Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post, List<IFormFile>? Photos, IFormFile? Model3D)
        {
            _logger.LogInformation("Starting post creation.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid.");

                foreach (var error in ModelState)
                {
                    foreach (var e in error.Value.Errors)
                    {
                        _logger.LogWarning($"Validation Error in {error.Key}: {e.ErrorMessage}");
                    }
                }

                return View(post);
            }

            post.IsApproved = false;

            
            var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var allowed3DExtensions = new[] { ".glb", ".gltf", ".obj", ".stl" };

            
            if (Photos == null) Photos = new List<IFormFile>();

            List<string> uploadedPaths = new List<string>();

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            var modelsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/models");

            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            if (!Directory.Exists(modelsFolder)) Directory.CreateDirectory(modelsFolder);

            
            foreach (var photo in Photos)
            {
                var fileExtension = Path.GetExtension(photo.FileName).ToLower();
                if (!allowedImageExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Photos", "❌ Зөвхөн JPG, JPEG, PNG, GIF файл зөвшөөрөгдөнө.");
                    return View(post);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                uploadedPaths.Add("/uploads/" + uniqueFileName);
            }

            string? model3DPath = null;
            if (Model3D != null)
            {
                var modelExtension = Path.GetExtension(Model3D.FileName).ToLower();
                if (!allowed3DExtensions.Contains(modelExtension))
                {
                    ModelState.AddModelError("Model3D", "❌ Зөвхөн .glb, .gltf, .obj, .stl файл зөвшөөрөгдөнө.");
                    return View(post);
                }

                var uniqueModelName = Guid.NewGuid().ToString() + modelExtension;
                var modelPath = Path.Combine(modelsFolder, uniqueModelName);

                using (var stream = new FileStream(modelPath, FileMode.Create))
                {
                    await Model3D.CopyToAsync(stream);
                }

                model3DPath = "/models/" + uniqueModelName;
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogError("User not found.");
                return Unauthorized();
            }

            post.CreatedBy = userId;
            post.CreatedDate = DateTime.UtcNow;
            post.PhotoPaths = uploadedPaths;
            post.PhotoPathsJson = JsonSerializer.Serialize(uploadedPaths);
            post.Model3DPath = model3DPath;

            _context.Add(post);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Post created successfully.");
            return RedirectToAction(nameof(Index));
        }

        
        [AllowAnonymous]
        public async Task<IActionResult> Index(string district, string priceRange, string condition)
        {
            var posts = _context.Posts.AsQueryable();

            
            ViewBag.Districts = await _context.Posts
                .Select(p => p.Location)  
                .Distinct()
                .ToListAsync();

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);  
                posts = posts.Where(p => p.CreatedBy == userId || p.IsApproved == true);
            }

            if (!string.IsNullOrEmpty(district))
            {
                posts = posts.Where(p => p.Location.Contains(district));
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                var priceRangeParts = priceRange.Split('-');
                var minPrice = int.Parse(priceRangeParts[0]);
                var maxPrice = priceRangeParts.Length > 1 ? int.Parse(priceRangeParts[1]) : int.MaxValue;
                posts = posts.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
            }

            if (!string.IsNullOrEmpty(condition))
            {
                posts = posts.Where(p => p.Condition == condition);
            }

            var model = await posts.ToListAsync();
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
        // Admin харуулах - зөвхөн батлагдаагүй зарууд
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PendingPosts()
        {
            var posts = await _context.Posts
                .Where(p => p.IsApproved == false)
                .ToListAsync();
            return View(posts);
        }

        // Admin батлах
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            post.IsApproved = true;
            post.RejectReason = null;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(PendingPosts));
        }

        // Admin татгалзах
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            post.IsApproved = false;
            post.RejectReason = reason;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(PendingPosts));
        }
        [AllowAnonymous]
        public IActionResult HowToUpload3D()
        {
            return View();
        }
    }
}