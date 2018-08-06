using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crawler
{
	class Program
	{
		static readonly Dictionary<Uri, Page> DomainPages = new Dictionary<Uri, Page>();
		
		static void Main(string[] args)
		{
			//Obvious future enhancement, get domain via args
			var domain = new Uri("https://hirespace.com/");

			var runner = new CrawlRunner(domain);

			Task.Run(() => runner.Crawl()).Wait();

			Console.ReadLine();
		}
	}
}
