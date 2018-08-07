using Xunit;
using Crawler;
using System;

namespace Crawler.Tests
{
    public class UrlSanitiserTests
    {
		private static Uri testBaseUri = new Uri("https://testing.com");

        [Fact]
		public void UrlSanitiser_Absolute_Local()
		{
			var sanitiser = new UrlSanitiser(testBaseUri);
			var sanitised = sanitiser.SanitiseLocal("https://testing.com/example");
			Assert.Equal("https://testing.com/example", sanitised.ToString());
		}

		[Fact]
		public void UrlSanitiser_Absolute_Remote()
		{
			var sanitiser = new UrlSanitiser(testBaseUri);
			var sanitised = sanitiser.SanitiseLocal("https://testing2.com/example");
			Assert.Null(sanitised);
		}

		[Fact]
		public void UrlSanitiser_Relative()
		{
			var sanitiser = new UrlSanitiser(testBaseUri);
			var sanitised = sanitiser.SanitiseLocal("example");
			Assert.Equal("https://testing.com/example", sanitised.ToString());
		}

		[Fact]
		public void UrlSanitiser_Absolute_Fragment()
		{
			var sanitiser = new UrlSanitiser(testBaseUri);
			var sanitised = sanitiser.SanitiseLocal("https://testing.com/example#fragment");
			Assert.Equal("https://testing.com/example", sanitised.ToString());
		}

		[Fact]
		public void UrlSanitiser_Relative_Fragment()
		{
			var sanitiser = new UrlSanitiser(testBaseUri);
			var sanitised = sanitiser.SanitiseLocal("example#fragment");
			Assert.Equal("https://testing.com/example", sanitised.ToString());
		}

		[Fact]
		public void UrlSanitiser_Differing_Scheme()
		{
			var sanitiser = new UrlSanitiser(testBaseUri);
			var sanitised = sanitiser.SanitiseLocal("http://testing.com/example");
			Assert.Null(sanitised);
		}
	}
}
