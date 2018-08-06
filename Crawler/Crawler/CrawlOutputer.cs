using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;

namespace Crawler
{
    public class CrawlOutputer
    {
		private IDictionary<Uri, Page> CrawlResults { get; }

		public CrawlOutputer(IDictionary<Uri, Page> crawlResults)
		{
			CrawlResults = crawlResults;
		}

		public void CreateReport()
		{
			StringBuilder report = new StringBuilder();

			report.Append("<!doctype><html><head></head><body><h1>Crawl Report</h1>");
			report.Append("<ul>");
			foreach (var page in CrawlResults.OrderBy(x => x.Key.ToString()))
			{
				report.Append($@"
				<li class=""page"">
					<h4 class=""url"">{WebUtility.HtmlEncode(page.Value.Url.ToString())}</h4>
					<div class=""title"">{WebUtility.HtmlEncode(page.Value.Title)}</div>
					<div class=""status"">{WebUtility.HtmlEncode(page.Value.Result.ToString())}</div>
					<h5>Inlinks</h5>
				");

				report.Append(@"<ul class=""inlinks"">");
				foreach (var inlink in page.Value.InLinks.OrderBy(x=>x.ToString()))
				{
					report.Append($@"<li class=""inlink"">{WebUtility.HtmlEncode(inlink.ToString())}</li>");
				}
				report.Append(@"</ul>");

				if (page.Value.Result == CrawlResult.Error)
				{
					report.Append($@"<div class=""error"">{WebUtility.HtmlEncode(page.Value.Exception.ToString())}</div>");
				}

				if (page.Value.Result == CrawlResult.Success && page.Value.OutLinks.Any())
				{
					report.Append(@"<h5>Outlinks</h5><ul class=""outlinks"">");
					foreach (var outlink in page.Value.OutLinks.OrderBy(x=>x.ToString()))
					{
						var outlinkStatus = CrawlResults.ContainsKey(outlink) ? CrawlResults[outlink].Result.ToString() : "Not in list";
						report.Append($@"<li class=""outlink""><span class=""outlink-url"">{WebUtility.HtmlEncode(outlink.ToString())}</span> <span class=""outlink-status"">{WebUtility.HtmlEncode(outlinkStatus)}</span></li>");
					}
					report.Append(@"</ul>");
				}

				report.Append($@"</li>");
			}
			report.Append("</ul>");

			using (var output = File.CreateText("report.html"))
			{
				output.Write(report.ToString());
			}
		}
	}
}
