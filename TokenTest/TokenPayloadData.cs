using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenTest
{
    public class TokenPayloadData
    {
        public string iss { get; set; }
        public string sub { get; set; }
        public long exp { get; set; }
        public long iat { get; set; }
        public string scope { get; set; }
    }
}
