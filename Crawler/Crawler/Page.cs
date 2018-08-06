using System;
using System.Collections.Generic;

namespace Crawler
{
    public class Page
    {
        public Uri Url { get; set; }
        public String Title { get; set; }
        public IList<Uri> InLinks { get; set; }
        public IEnumerable<Uri> OutLinks { get; set; }
        public CrawlStatus Status { get; set; }
		public Exception Exception { get; set; }
    }
}
