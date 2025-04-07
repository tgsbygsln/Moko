using System.Collections.Generic;

namespace AirFastNew.Models
{
    public class PostDetailsViewModel
    {
        public required Post Post { get; set; }
        public List<Post> OtherPosts { get; set; } = new List<Post>();

    }
}