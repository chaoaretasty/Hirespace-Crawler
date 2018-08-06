using System;
using System.Collections.Generic;

namespace Crawler
{
    public class Page
    {
        public Uri Url { get; set; }
        public String Title { get; set; }
        public IEnumerable<Uri> InLinks { get; set; }
        public IEnumerable<Uri> OutLinks { get; set; }
        public CrawlResult Result { get; set; }
		public Exception Exception { get; set; }
    }
}
