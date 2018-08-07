using System;
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

			private string IndexPage = @"<!doctype>
<html>
	<head><title>Index page</title></head>
	<body>
		<a href=""first.html""></a>
		<a href=""second.html""></a>
		<a href=""error.html""></a>
	</body>
</html>";

			private string FirstPage = @"<!doctype>
<html>
	<head><title>First page</title></head>
	<body>
		<a href=""""></a>
	</body>
</html>";

			private string SecondPage = @"<!doctype>
<html>
	<head><title>Second page</title></head>
	<body>
		<a href=""""></a>
		<a href=""first.html""></a>
		<a href=""first.html""></a>
		<a href=""https://testing2.com/blah""></a>
	</body>
</html>";
		}

		[Fact]
		public async void Parse_Title()
		{
			var parser = A.Fake<HtmlParser>(x => x.CallsBaseMethods());
			var runner = new CrawlRunner(baseUrl, new HttpClient(new FakeHandler()), parser);
			var results = await runner.Crawl();

			Assert.Equal("Index page", results[new Uri("https://testing.com")].Title);
			Assert.Equal("First page", results[new Uri("https://testing.com/first.html")].Title);
			Assert.Equal("Second page", results[new Uri("https://testing.com/second.html")].Title);
		}

		[Fact]
		public void Parse_Links()
		{
			
		}

		[Fact]
		public void Parse_Links_Duplicate()
		{

		}

		[Fact]
		public void Parse_Links_Sanitises()
		{

		}

		[Fact]
		public void Crawl()
		{
			
		}

		[Fact]
		public void Crawl_Error()
		{

		}

		private Uri baseUrl = new Uri("https://testing.com");

		
	}
}
