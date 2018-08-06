using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Crawler
{
    public class CrawlRunner
    {
		private readonly IDictionary<Uri, Page> _crawlCollection = new Dictionary<Uri, Page>();
		private readonly HttpClient client = new HttpClient();
		private readonly HtmlParser Parser = new HtmlParser();
		private UrlSanitiser Sanitiser;

		public CrawlRunner(Uri root)
		{
			Sanitiser = new UrlSanitiser(root.Host, root.Scheme);
			AddUrlToCrawl(root);
			client.DefaultRequestHeaders.Add("user-agent", "Coding Sample Crawler");
		}
		
		public async Task<IDictionary<Uri, Page>> Crawl()
		{
			while(_crawlCollection.Any(x => x.Value.Result == CrawlResult.Waiting))
			{
				var nextpage = _crawlCollection.First(y => y.Value.Result == CrawlResult.Waiting).Value;
				
				var crawled = await CrawlPage(nextpage);
				_crawlCollection.Remove(nextpage.Url);
				_crawlCollection.Add(crawled.Url, crawled);

				foreach (var newpage in (crawled.OutLinks ?? Enumerable.Empty<Uri>())) { AddUrlToCrawl(newpage); }

#if DEBUG
				if (_crawlCollection.Count(x=>x.Value.Result == CrawlResult.Success) > 20) { break; }
#endif
			}

			return _crawlCollection;
		}

		private void AddUrlToCrawl(Uri newpage)
		{
			if (!_crawlCollection.ContainsKey(newpage))
			{
				_crawlCollection.Add(newpage, new Page
				{
					Url = newpage,
					Result = CrawlResult.Waiting
				});
			}
		}

		private async Task<Page> CrawlPage(Page page)
		{
			Console.WriteLine("Crawling: " + page.Url);
			Page parsedPage;
			try
			{
				var crawled = await client.GetStringAsync(page.Url);
				parsedPage = await ParseResult(crawled);
				parsedPage.Result = CrawlResult.Success;
			}
			catch(Exception e)
			{
				parsedPage = new Page
				{
					Result = CrawlResult.Error,
					Exception = e
				};
			}
			parsedPage.Url = page.Url;
			return parsedPage;
		}

		private async Task<Page> ParseResult(string bodyContent)
		{
			var parsed = await Parser.ParseAsync(bodyContent);

			var page = new Page { Title = parsed.Title };

			page.OutLinks = parsed.Body.QuerySelectorAll("a")
				.Select(elem => elem.Attributes.SingleOrDefault(attr => attr.Name == "href" && !string.IsNullOrWhiteSpace(attr.Value))?.Value)
				.Select(Sanitiser.SanitiseLocal).Where(x => x != null);

			return page;
		}
	}
}
