using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaTALK.MVVM.Models
{
    public class Post
    {
        public DateTime postedAt { get; set; }
        public int userId { get; set; }
        public string content { get; set; }
        public DateTime modifiedAt { get; set; }
        public int likes { get; set; }
        public object[] comments { get; set; }
        public int shares { get; set; }
        public string caption { get; set; }
        public bool isArchived { get; set; }
        public string id { get; set; }
    }

}
