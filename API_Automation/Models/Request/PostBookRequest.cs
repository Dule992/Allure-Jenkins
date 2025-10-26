using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_Automation.Models.Request
{
    public class PostBookRequest
    {
        public string userId { get; set; }
        public List<Isbns> collectionOfIsbns { get; set; }

        public class Isbns()
        {
            public string isbn { get; set; }
        }
    }
}
