using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_Automation.Models.Response
{
    public class BooksPostResponse
    {
        public List<Book> books { get; set; }

        public class Book
        {
            public string isbn { get; set; }
        }
    }
}
