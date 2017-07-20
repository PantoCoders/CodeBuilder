using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Panto.Framework.MVC
{
	public sealed class Http404Result : IActionResult
	{
		public void Ouput(HttpContext context)
		{
			context.Response.StatusCode = 404;
		}

	}
}
