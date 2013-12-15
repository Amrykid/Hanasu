using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystal.Core;

namespace Hanasu.Model
{
    public class NewsItem: BaseModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public DateTime PublishDate { get; set; }
    }
}
