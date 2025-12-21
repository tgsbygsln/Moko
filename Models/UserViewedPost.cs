using System;

namespace AirFastNew.Models
{
    public class UserViewedPost
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int PostId { get; set; }
        public DateTime ViewedAt { get; set; }
    }
}