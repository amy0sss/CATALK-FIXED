using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaTALK.MVVM.Models
{
    public class User
    {
        public DateTime createdAt { get; set; }
        public string avatar { get; set; }
        public string password { get; set; }
        public string username { get; set; }
        public object[] friends { get; set; }
        public DateTime modifiedAt { get; set; }
        public bool isActive { get; set; }
        public DateTime birthday { get; set; }
        public object[] friendRequests { get; set; }
        public string id { get; set; }
    }
}
