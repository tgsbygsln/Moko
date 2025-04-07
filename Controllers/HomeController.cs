using Microsoft.AspNetCore.Mvc;
using AirFastNew.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

namespace AirFastNew.Controllers
{
    public class HomeController : Controller
    {
        private readonly AirFastDbContext _context;

        public HomeController(AirFastDbContext context)
        {
            _context = context;
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "Холбоо барих мэдээлэл";
            return View();
        }

        public IActionResult Index(string district, string priceRange, string condition, string sortBy, string type, int page = 1, int pageSize = 8)
        {
            var posts = _context.Posts.AsQueryable();

            // Preserve filter selections
            ViewBag.SelectedDistrict = district;
            ViewBag.SelectedPriceRange = priceRange;
            ViewBag.SelectedCondition = condition;
            ViewBag.SelectedSortBy = sortBy;
            ViewBag.SelectedType = type;

            // Apply filters
            if (!string.IsNullOrEmpty(district))
                posts = posts.Where(p => p.Location == district);

            if (!string.IsNullOrEmpty(condition))
                posts = posts.Where(p => p.Condition == condition);

            if (!string.IsNullOrEmpty(priceRange))
            {
                var priceParts = priceRange.Split('-');
                if (priceParts.Length == 2 && decimal.TryParse(priceParts[0], out decimal minPrice) &&
                    decimal.TryParse(priceParts[1], out decimal maxPrice))
                {
                    posts = posts.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
                }
                else if (priceRange == "550'000'000+")
                {
                    posts = posts.Where(p => p.Price >= 550000000);
                }
            }

            // Rent/Sale Filtering
            if (type == "rent")
            {
                posts = posts.Where(p => p.PostType == "rent");
            }
            else if (type == "sale")
            {
                posts = posts.Where(p => p.PostType == "sale");
            }

            // Sorting
            posts = sortBy switch
            {
                "priceAsc" => posts.OrderBy(p => p.Price),
                "priceDesc" => posts.OrderByDescending(p => p.Price),
                "newest" => posts.OrderByDescending(p => p.CreatedDate), // Fixed property name
                _ => posts.OrderByDescending(p => p.CreatedDate) // Default: newest first
            };

            // Pagination
            int totalPosts = posts.Count();
            int totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);

            var paginatedPosts = posts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(paginatedPosts);
        }
    }
}