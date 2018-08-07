using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Net.Http;
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

			var client = new HttpClient();
			var parser = new HtmlParser();

			var runner = new CrawlRunner(domain, client, parser);

			var runnerTask = Task.Run(() => runner.Crawl());
			runnerTask.Wait();

			new CrawlOutputer(runnerTask.Result).CreateReport();

			Console.WriteLine("Crawl complete, press enter to close");
			Console.ReadLine();
		}
	}
}
