using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class UrlSanitiser
    {
		private readonly string _localHostname;
		private readonly string _scheme;
		private Uri _baseUri;

		public UrlSanitiser(string localHostname, string scheme)
		{
			_localHostname = localHostname;
			_scheme = scheme;
			_baseUri = new Uri(_scheme + "://" + _localHostname);
		}

		public Uri SanitiseLocal(string url)
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
				return (uri.Authority == _baseUri.Authority) ? uri : null;
			}
			
			return null;
		}
    }
}
