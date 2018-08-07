using System;

namespace Crawler
{
	//This class inspects links parsed from a page and checks to see if it is local to the given domain
	//It also sanitises links as not all links as written in HTML would be appropriate (eg javascript void, malformed urls)
	//Url fragments have been removed as these do not refer to separate pages (barring certain front-end frameworks, the parsing of which are beyond the scope of this project)
	public class UrlSanitiser
    {
		private Uri _baseUri;

		public UrlSanitiser(Uri baseUri)
		{
			_baseUri = baseUri;
		}

		public virtual Uri SanitiseLocal(string url)
		{
			if(url == null) { return null; }

			var fragmentPos = url.IndexOf('#');
			if(fragmentPos > -1)
			{
				url = url.Remove(fragmentPos);
			}

			Uri uri;

			if (Uri.TryCreate(url, UriKind.Relative, out uri))
			{
				return new Uri(_baseUri, url);
			}

			if(Uri.TryCreate(url, UriKind.Absolute, out uri))
			{
				return (uri.GetLeftPart(UriPartial.Authority) == _baseUri.GetLeftPart(UriPartial.Authority)) ? uri : null;
			}
			
			return null;
		}
    }
}
