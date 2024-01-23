using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSwitcher.Models
{
    public struct Account
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string UID { get; set; }
        public int Level { get; set; }
        public string Server { get; set; }
        public string Token { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
