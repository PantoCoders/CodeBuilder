using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using System.Reflection;

namespace Panto.Map.Extensions
{
	internal sealed class ContentEncodingModule : IHttpModule
	{
		public void Init(HttpApplication app)
		{
			app.PostResolveRequestCache += new EventHandler(app_PostResolveRequestCache);
		}

		void app_PostResolveRequestCache(object sender, EventArgs e)
		{
			HttpApplication app = (HttpApplication)sender;
			HttpWorkerRequest request = (((IServiceProvider)app.Context).GetService(typeof(HttpWorkerRequest)) as HttpWorkerRequest);

			string method = request.GetHttpVerbName();

			if( method == "GET" ) {

				byte[] queryStringBytes = request.GetQueryStringRawBytes();

				if( queryStringBytes == null ) {
					return;
				}

				NameValueCollection values = null;
				try {
					string queryString = Encoding.UTF8.GetString(queryStringBytes);
					values = HttpUtility.ParseQueryString(queryString, Encoding.UTF8);
				}
				catch( DecoderFallbackException ) {
					return;
				}

				string charset = values["x-charset"];

				if( charset == null ) {
					return;
				}

				if( charset.IndexOf("utf-8", StringComparison.OrdinalIgnoreCase ) >= 0 )
					SetRequest(app.Request);
			}
			else if ( method == "POST" ) {

				string charset = request.GetUnknownRequestHeader("x-charset");

				if( charset == null ) {
					return;
				}

				if( string.Compare(charset, "utf-8", StringComparison.OrdinalIgnoreCase) == 0 )
					SetRequest(app.Request);
			}
		}

		private void SetRequest(HttpRequest request)
		{
			request.ContentEncoding = System.Text.Encoding.UTF8;

			Type type = request.GetType();

			BindingFlags flag = BindingFlags.NonPublic | BindingFlags.Instance;

			FieldInfo fieldQueryString = type.GetField("_queryString", flag);
			if( fieldQueryString != null ) {
				fieldQueryString.FastSetField(request, null);
			}

			FieldInfo fieldForms = type.GetField("_form", flag);
			if( fieldForms != null ) {
				fieldForms.FastSetField(request, null);
			}
		}

		public void Dispose()
		{
		}

	}
}
