using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace AirFastNew.Models
{
    public class Post
    {
        public int Id { get; set; }

        [NotMapped]
        public List<string> PhotoPaths { get; set; } = new();

        public string PhotoPathsJson
        {
            get => PhotoPaths is { Count: > 0 } ? JsonSerializer.Serialize(PhotoPaths) : "[]";
            set => PhotoPaths = string.IsNullOrEmpty(value) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(value) ?? new List<string>();
        }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Size must be greater than zero.")]
        [Display(Name = "Size (m²)")]
        public double SquareMeters { get; set; }

        [Required]
        [Display(Name = "Condition")]
        public string Condition { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string? Model3DPath { get; set; }

        [Required(ErrorMessage = "Зарын төрлийг сонгоно уу.")]
        public string PostType { get; set; } = string.Empty;
        public virtual ApplicationUser? CreatedByUser { get; set; }

        private static readonly Dictionary<string, string> ConditionDisplayNames = new()
        {
            { "New", "Шинэ" },
            { "Good", "Хуучин" },
            { "Average", "Засвартай" },
            { "Old", "Засвар хийх шаардлагатай" }
        };

        public string GetConditionDisplay()
        {
            return ConditionDisplayNames.TryGetValue(Condition, out var displayName)
                ? displayName
                : "Unknown Condition";
        }

        public string? PhoneNumber => CreatedByUser?.PhoneNumber;
    }
}