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
		private readonly HttpClient _client;
		private readonly HtmlParser _parser;
		private readonly UrlSanitiser _sanitiser;

		public CrawlRunner(Uri root, HttpClient client, HtmlParser parser, UrlSanitiser sanitiser)
		{
			_sanitiser = sanitiser;
			_client = client;
			_parser = parser;
			AddUrlToCrawl(root);
			client.DefaultRequestHeaders.Add("user-agent", "Coding Sample Crawler");
		}
		
		public async Task<IDictionary<Uri, Page>> Crawl()
		{
			// Currently just iterating individually through the list
			// The crawl runner code is async throughout to enable running this on multiple tasks and the 
			// Will need to manage the number of concurrent tasks running rather than letting them them all run as new URLs are discovered
			// This will prevent memory exhaustion from excessive numbers of tasks and avoid being denied access for sites implementing rate limiting

			while(_crawlCollection.Any(x => x.Value.Status == CrawlStatus.Waiting))
			{
				var nextpage = _crawlCollection.First(y => y.Value.Status == CrawlStatus.Waiting).Value;
				
				var crawled = await CrawlPage(nextpage);
				_crawlCollection[nextpage.Url] = crawled;

				foreach (var newpage in (crawled.OutLinks ?? Enumerable.Empty<Uri>())) { AddUrlToCrawl(newpage); }

				#if DEBUG
					if (_crawlCollection.Count(x=>x.Value.Status == CrawlStatus.Success) > 20) { break; }
				#endif
			}

			#if DEBUG
				// Uncrawled pages are not initialised so need to be cleaned up or there will be exceptions thrown when accessing collections
				// As this issue only affects debug it makes more sense to cleanup here than later
				var toRemove = _crawlCollection.Where(x => x.Value.Status == CrawlStatus.Waiting || x.Value.Status == CrawlStatus.Processing).ToList();
				foreach(var x in toRemove)
				{
					_crawlCollection.Remove(x);
				}
			#endif

			foreach(var page in _crawlCollection)
			{
				foreach(var outlink in page.Value.OutLinks ?? Enumerable.Empty<Uri>())
				{
					if (_crawlCollection.ContainsKey(outlink))
					{
						var inlinks = _crawlCollection[outlink].InLinks;
						if (!inlinks.Contains(page.Key)) {
							inlinks.Add(page.Key);
						}
					}
				}
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
					Status = CrawlStatus.Waiting
				});
			}
		}

		private async Task<Page> CrawlPage(Page page)
		{
			Console.WriteLine("Crawling: " + page.Url);
			Page parsedPage;
			try
			{
				var crawled = await _client.GetStringAsync(page.Url);
				parsedPage = await ParseResult(crawled);
				parsedPage.Status = CrawlStatus.Success;
			}
			// Currently no special handling for any particular exception
			// Future improvements would include capturing various sorts of exceptions to potentially requeue for crawling
			catch(Exception e)
			{
				parsedPage = new Page
				{
					Status = CrawlStatus.Error,
					Exception = e
				};
			}
			parsedPage.Url = page.Url;
			parsedPage.InLinks = new List<Uri>();
			return parsedPage;
		}

		private async Task<Page> ParseResult(string bodyContent)
		{
			var parsed = await _parser.ParseAsync(bodyContent);

			var page = new Page { Title = parsed.Title };

			page.OutLinks = parsed.Body.QuerySelectorAll("a")
				.Select(elem => elem.Attributes.SingleOrDefault(attr => attr.Name == "href" && !string.IsNullOrWhiteSpace(attr.Value))?.Value)
				.Select(_sanitiser.SanitiseLocal)
				.Where(x => x != null)
				.Distinct();

			return page;
		}
	}
}
