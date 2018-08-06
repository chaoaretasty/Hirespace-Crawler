using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using System.Net.Http;

namespace Crawler
{
	class Program
	{
		static readonly Dictionary<Uri, Page> DomainPages = new Dictionary<Uri, Page>();
		static readonly HtmlParser Parser = new HtmlParser();
		static string Host;
		static string Scheme;
		static UrlSanitiser Sanitiser;

		static void Main(string[] args)
		{
			//Obvious future enhancement, get domain via args
			var domain = new Uri("https://hirespace.com/");
			Host = domain.Host;

			//For now considering local to the domain to include the scheme
			Scheme = domain.Scheme;

			Sanitiser = new UrlSanitiser(Host, Scheme);

			DomainPages.Add(domain, new Page { Url = domain, Result = CrawlResult.Waiting });

			var client = new HttpClient();
			client.GetStringAsync(domain)
				.ContinueWith(async x => {
					var thispage = await ParseResult(x.Result);
					Console.WriteLine($"Title: {thispage.Title}");
					Console.WriteLine("Outlinks:");
					foreach(var outlink in thispage.OutLinks)
					{
						Console.WriteLine(outlink);
					}
				});

			Console.ReadLine();
		}

		static async Task<Page> ParseResult(string bodyContent)
		{
			var parsed = await Parser.ParseAsync(bodyContent);

			var page = new Page{ Title = parsed.Title };

			page.OutLinks = parsed.Body.QuerySelectorAll("a")
				.Select(elem => elem.Attributes.SingleOrDefault(attr => attr.Name == "href" && !string.IsNullOrWhiteSpace(attr.Value))?.Value)
				.Select(Sanitiser.SanitiseLocal).Where(x => x != null);
			
			return page;
		}
	}
}
