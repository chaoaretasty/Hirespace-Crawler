using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using FakeItEasy;
using Xunit;

namespace Crawler.Tests
{
    public class CrawlRunnerTests
    {
		[Fact]
		public async void Parse_Title()
		{
			var runner = new CrawlRunner(_baseUrl, new HttpClient(new FakeHandler()), _parser, _sanitiser);
			var results = await runner.Crawl();

			Assert.Equal("Index page", results[new Uri("https://testing.com")].Title);
			Assert.Equal("First page", results[new Uri("https://testing.com/first.html")].Title);
			Assert.Equal("Second page", results[new Uri("https://testing.com/second.html")].Title);
		}

		[Fact]
		public async void Parse_Links()
		{
			var runner = new CrawlRunner(_baseUrl, new HttpClient(new FakeHandler()), _parser, _sanitiser);
			var results = await runner.Crawl();

			var expectedLinks = new List<string> { "https://testing.com/first.html", "https://testing.com/second.html", "https://testing.com/error.html" };
			var actualLinks = results[new Uri("https://testing.com")].OutLinks.Select(x => x.ToString()).ToList();
			Assert.Equal(expectedLinks, actualLinks);
		}

		[Fact]
		public async void Parse_Links_Duplicate()
		{
			var runner = new CrawlRunner(_baseUrl, new HttpClient(new FakeHandler()), _parser, _sanitiser);
			var results = await runner.Crawl();

			var expectedLinks = new List<string> { "https://testing.com/", "https://testing.com/first.html" };
			var actualLinks = results[new Uri("https://testing.com/second.html")].OutLinks.Select(x => x.ToString()).ToList();
			Assert.Equal(expectedLinks, actualLinks);
		}

		[Fact]
		public async void Parse_Links_Sanitises()
		{
			var fakesanitiser = A.Fake<UrlSanitiser>(x => x.CallsBaseMethods().WithArgumentsForConstructor(() => new UrlSanitiser(_baseUrl)));
			var runner = new CrawlRunner(_baseUrl, new HttpClient(new FakeHandler()), _parser, fakesanitiser);
			var results = await runner.Crawl();

			A.CallTo(() => fakesanitiser.SanitiseLocal(A<string>.Ignored)).MustHaveHappened();
		}

		[Fact]
		public async void Crawl()
		{
			var runner = new CrawlRunner(_baseUrl, new HttpClient(new FakeHandler()), _parser, _sanitiser);
			var results = await runner.Crawl();

			var testPage = new Uri("https://testing.com/");
			var expectedPage = new Page
			{
				InLinks = new List<Uri> { new Uri("https://testing.com/second.html") },
				OutLinks = new List<Uri> { new Uri("https://testing.com/first.html"), new Uri("https://testing.com/second.html"), new Uri("https://testing.com/error.html") },
				Status = CrawlStatus.Success,
				Title = "Index page",
				Url = testPage
			};

			Assert.Equal(expectedPage.Title, results[testPage].Title);
			Assert.Equal(expectedPage.Status, results[testPage].Status);
			Assert.Equal(expectedPage.Url, results[testPage].Url);
			Assert.Equal(expectedPage.OutLinks, results[testPage].OutLinks);
			Assert.Equal(expectedPage.InLinks, results[testPage].InLinks);
		}

		[Fact]
		public async void Crawl_Error()
		{
			var runner = new CrawlRunner(_baseUrl, new HttpClient(new FakeHandler()), _parser, _sanitiser);
			var results = await runner.Crawl();

			var testPage = new Uri("https://testing.com/error.html");
			var expectedPage = new Page
			{
				InLinks = new List<Uri> { new Uri("https://testing.com/") },
				OutLinks = null,
				Status = CrawlStatus.Error,
				Title = null,
				Url = testPage
			};

			Assert.Equal(expectedPage.Title, results[testPage].Title);
			Assert.Equal(expectedPage.Status, results[testPage].Status);
			Assert.Equal(expectedPage.Url, results[testPage].Url);
			Assert.Equal(expectedPage.OutLinks, results[testPage].OutLinks);
			Assert.Equal(expectedPage.InLinks, results[testPage].InLinks);
		}

		private static Uri _baseUrl = new Uri("https://testing.com");
		//Ideally would like to test for a call to the parse method but it is not virtual and is external code
		private static HtmlParser _parser = new HtmlParser();
		private static UrlSanitiser _sanitiser = new UrlSanitiser(_baseUrl);

		private class FakeHandler : HttpMessageHandler
		{
			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				var response = new HttpResponseMessage();
				response.StatusCode = System.Net.HttpStatusCode.OK;

				switch (request.RequestUri.ToString())
				{
					case "https://testing.com/":
						response.Content = new StringContent(IndexPage);
						break;
					case "https://testing.com/first.html":
						response.Content = new StringContent(FirstPage);
						break;
					case "https://testing.com/second.html":
						response.Content = new StringContent(SecondPage);
						break;
					case "https://testing.com/error.html":
						response.StatusCode = System.Net.HttpStatusCode.NotFound;
						break;
				}
				return Task.FromResult(response);
			}

			private const string IndexPage = @"<!doctype>
<html>
	<head><title>Index page</title></head>
	<body>
		<a href=""first.html""></a>
		<a href=""second.html""></a>
		<a href=""error.html""></a>
	</body>
</html>";

			private const string FirstPage = @"<!doctype>
<html>
	<head><title>First page</title></head>
	<body>
		<a href=""second.html#fragment""></a>
	</body>
</html>";

			private const string SecondPage = @"<!doctype>
<html>
	<head><title>Second page</title></head>
	<body>
		<a href=""/""></a>
		<a href=""first.html""></a>
		<a href=""first.html""></a>
		<a href=""https://testing2.com/blah""></a>
	</body>
</html>";
		}
	}
}
