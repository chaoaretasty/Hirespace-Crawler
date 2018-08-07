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

			//Whether the sanitiser should be owned by the crawler or not for a larger solution is questionable,
			//you may wish differing sanitiser behaviours regarding eg casing or trailing slashes
			//In a larger project where this is the case you would ideally have unit tests for sanitiser separate and only test the crawler called it
			var sanitiser = new UrlSanitiser(domain);
			
			var runner = new CrawlRunner(domain, client, parser, sanitiser);

			var runnerTask = Task.Run(() => runner.Crawl());
			runnerTask.Wait();

			new CrawlOutputer(runnerTask.Result).CreateReport();

			Console.WriteLine("Crawl complete, press enter to close");
			Console.ReadLine();
		}
	}
}
