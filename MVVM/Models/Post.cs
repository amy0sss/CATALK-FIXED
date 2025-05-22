using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CaTALK.MVVM.Models
{
    public class Post
    {
        public string id { get; set; }
        public string userId { get; set; }
        public string content { get; set; }
        public string caption { get; set; }
        public DateTime postedAt { get; set; }
        public DateTime modifiedAt { get; set; }
        public int likes { get; set; }
        public Comment[] comments { get; set; }  // array of comment objects
        public int shares { get; set; }
        public bool isArchived { get; set; }

        // This is a computed property, not from JSON
        [JsonIgnore]
        public string TimeAgo { get; set; }
        [JsonIgnore]
        public string PostAvatar { get; set; }
    }

    public class Comment
    {
        public string id { get; set; }
        public string text { get; set; }
    }
}
